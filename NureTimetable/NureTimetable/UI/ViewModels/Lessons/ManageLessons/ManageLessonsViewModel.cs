using NureTimetable.Core.Extensions;
using NureTimetable.Core.Localization;
using NureTimetable.Core.Models.Consts;
using NureTimetable.DAL;
using NureTimetable.DAL.Models.Local;
using NureTimetable.UI.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace NureTimetable.UI.ViewModels
{
    public class ManageLessonsViewModel : BaseViewModel
    {
        private Entity entity;

        #region Properties
        public bool HasUnsavedChanges { get; set; } = false;

        public ObservableRangeCollection<LessonViewModel> Lessons { get; } = new();

        public IAsyncCommand PageAppearingCommand { get; }
        public IAsyncCommand SaveClickedCommand { get; }
        public IAsyncCommand BackButtonPressedCommand { get; }
        #endregion

        public ManageLessonsViewModel(Entity entity)
        {
            Title = new(() => $"{LN.Lessons}: {entity.Name}");

            this.entity = entity;
            PageAppearingCommand = CommandFactory.Create(PageAppearing);
            BackButtonPressedCommand = CommandFactory.Create(BackButtonPressed);
            SaveClickedCommand = CommandFactory.Create(SaveClicked, () => Lessons.Any(), allowsMultipleExecutions: false);

            Lessons.CollectionChanged += (_, _) =>
            {
                SaveClickedCommand.RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(Lessons));
            };
        }

        private async Task PageAppearing()
        {
            if (Lessons.Any())
                return;

            TimetableInfo timetable = await EventsRepository.GetTimetableLocalAsync(entity);
            if (timetable == null)
            {
                await Shell.Current.DisplayAlert(LN.LessonsManagement, LN.AtFirstLoadTimetable, LN.Ok);
                await Navigation.PopAsync();
                return;
            }
            
            Lessons.ReplaceRange
            (
                timetable.Lessons()
                    .Select(lesson => timetable.LessonsInfo.FirstOrDefault(li => li.Lesson == lesson) ?? new LessonInfo { Lesson = lesson })
                    .OrderBy(lesson => !timetable.Events.Where(e => e.Start >= DateTime.Now).Any(e => e.Lesson == lesson.Lesson))
                    .ThenBy(lesson => lesson.Lesson.ShortName)
                    .Select(lesson => new LessonViewModel(lesson, timetable, this))
            );
        }

        private async Task SaveClicked()
        {
            await EventsRepository.UpdateLessonsInfo(entity, Lessons.Select(l => l.LessonInfo).ToList());
            HasUnsavedChanges = false;

            await Shell.Current.GoToAsync("..", true);
            try
            {
                Shell.Current.CurrentPage.DisplayToastAsync(string.Format(LN.EntityLessonSettingsSaved, entity.Name)).Forget();
            }
            catch { } // TODO: Remove when https://github.com/xamarin/XamarinCommunityToolkit/issues/959 is fixed
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
                await Shell.Current.GoToAsync("..", true);
            }
        }
    }
}