using NureTimetable.Core.Localization;
using NureTimetable.Core.Models.Consts;
using NureTimetable.DAL;
using NureTimetable.DAL.Models.Local;
using NureTimetable.UI.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace NureTimetable.UI.ViewModels.Lessons.ManageLessons
{
    public class ManageLessonsViewModel : BaseViewModel
    {
        private readonly TimetableInfo timetable;

        #region Properties
        public bool HasUnsavedChanges { get; set; } = false;

        private bool _isNoSourceLayoutVisible;
        public bool IsNoSourceLayoutVisible { get => _isNoSourceLayoutVisible; set => SetProperty(ref _isNoSourceLayoutVisible, value); }

        public ObservableRangeCollection<LessonViewModel> Lessons { get; }

        public IAsyncCommand PageAppearingCommand { get; }
        public IAsyncCommand SaveClickedCommand { get; }
        public IAsyncCommand BackButtonPressedCommand { get; }
        #endregion

        public ManageLessonsViewModel(Entity entity)
        {
            Title = new(() => $"{LN.Lessons}: {entity.Name}");

            timetable = EventsRepository.GetTimetableLocal(entity);
            if (timetable is null)
            {
                IsNoSourceLayoutVisible = true;
            }
            else
            {
                Lessons = new
                (
                    timetable.Lessons()
                        .Select(lesson => timetable.LessonsInfo.FirstOrDefault(li => li.Lesson == lesson) ?? new LessonInfo { Lesson = lesson })
                        .OrderBy(lesson => !timetable.Events.Where(e => e.Start >= DateTime.Now).Any(e => e.Lesson == lesson.Lesson))
                        .ThenBy(lesson => lesson.Lesson.ShortName)
                        .Select(lesson => new LessonViewModel(lesson, timetable, this))
                );
                IsNoSourceLayoutVisible = Lessons.Count == 0;
            }

            PageAppearingCommand = CommandHelper.Create(PageAppearing);
            SaveClickedCommand = CommandHelper.Create(SaveClicked);
            BackButtonPressedCommand = CommandHelper.Create(BackButtonPressed);
        }

        private async Task PageAppearing()
        {
            if (Lessons is not null)
                return;

            await Shell.Current.DisplayAlert(LN.LessonsManagement, LN.AtFirstLoadTimetable, LN.Ok);
            await Navigation.PopAsync();
        }

        private async Task SaveClicked()
        {
            if (Lessons is null)
            {
                await Shell.Current.DisplayAlert(LN.LessonsManagement, LN.AtFirstLoadTimetable, LN.Ok);
                return;
            }

            EventsRepository.UpdateLessonsInfo(timetable.Entity, Lessons.Select(l => l.LessonInfo).ToList());
            HasUnsavedChanges = false;

            await Shell.Current.DisplayAlert(LN.SavingSettings, string.Format(LN.EntityLessonSettingsSaved, timetable.Entity.Name), LN.Ok);
            try
            {
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                MessagingCenter.Send(Application.Current, MessageTypes.ExceptionOccurred, ex);
            }
        }

        private async Task BackButtonPressed()
        {
            bool canClose = true;
            if (HasUnsavedChanges)
            {
                canClose = await Shell.Current.DisplayAlert(LN.UnsavedChangesTitle, LN.UnsavedChangesMessage, LN.Yes, LN.Cancel);
            }

            if (canClose)
            {
                await Navigation.PopAsync();
            }
        }
    }
}