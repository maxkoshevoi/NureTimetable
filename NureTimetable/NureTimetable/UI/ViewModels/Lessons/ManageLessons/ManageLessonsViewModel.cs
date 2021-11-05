using NureTimetable.Core.Extensions;
using NureTimetable.Core.Localization;
using NureTimetable.DAL.Cist;
using NureTimetable.DAL.Models;
using NureTimetable.DAL.Moodle;
using NureTimetable.DAL.Moodle.Models.Courses;
using NureTimetable.DAL.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace NureTimetable.UI.ViewModels
{
    public class ManageLessonsViewModel : BaseViewModel
    {
        private readonly Entity entity;

        #region Properties
        public bool HasUnsavedChanges { get; set; } = false;

        public ObservableRangeCollection<LessonViewModel> Lessons { get; } = new();

        public IAsyncCommand PageAppearingCommand { get; }
        public IAsyncCommand SaveClickedCommand { get; }
        public IAsyncCommand SyncDlNureClickedCommand { get; }
        public IAsyncCommand BackButtonPressedCommand { get; }
        #endregion

        public ManageLessonsViewModel(Entity entity)
        {
            Title = new(() => $"{LN.Lessons}: {entity.Name}");

            this.entity = entity;
            PageAppearingCommand = CommandFactory.Create(PageAppearing);
            BackButtonPressedCommand = CommandFactory.Create(BackButtonPressed);
            SaveClickedCommand = CommandFactory.Create(SaveClicked, () => Lessons.Any(), allowsMultipleExecutions: false);
            SyncDlNureClickedCommand = CommandFactory.Create(SyncDlNureClecked, () => SettingsRepository.Settings.DlNureUser != null, allowsMultipleExecutions: false);

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

            TimetableInfo? timetable = await EventsRepository.GetTimetableLocalAsync(entity);
            if (timetable == null)
            {
                await Shell.Current.DisplayAlert(LN.LessonsManagement, LN.AtFirstLoadTimetable, LN.Ok);
                await Navigation.PopAsync();
                return;
            }
            
            Lessons.ReplaceRange
            (
                timetable.Lessons()
                    .Select(lesson => timetable.LessonsInfo.FirstOrDefault(li => li.Lesson == lesson) ?? new LessonInfo(lesson))
                    .Where(lesson => timetable.Events.Where(e => e.Start >= DateTime.Today).Any(e => e.Lesson == lesson.Lesson))
                    .OrderBy(lesson => lesson.Lesson.ShortName)
                    .Select(lesson => new LessonViewModel(lesson, timetable, this))
            );
        }

        private async Task SaveClicked()
        {
            await EventsRepository.UpdateLessonsInfo(entity, Lessons.Select(l => l.LessonInfo).ToList());
            HasUnsavedChanges = false;

            await Shell.Current.GoToAsync("..", true);
            Shell.Current.CurrentPage.DisplayToastAsync(string.Format(LN.EntityLessonSettingsSaved, entity.Name)).Forget();
        }

        private async Task SyncDlNureClecked()
        {
            bool canProceed = await Shell.Current.DisplayAlert(LN.SyncDlNureLessonsTitle, LN.SyncDlNureLessonsMessage, LN.Yes, LN.Cancel);
            if (!canProceed)
            {
                return;
            }

            List<Lesson> enrolledLessons = await GetEnrolledLessonsAsync(Lessons.Select(l => l.LessonInfo.Lesson));
            foreach (var lesson in Lessons)
            {
                if (!enrolledLessons.Contains(lesson.LessonInfo.Lesson))
                {
                    lesson.IsChecked = false;
                }
            }

            static async Task<List<Lesson>> GetEnrolledLessonsAsync(IEnumerable<Lesson> lessons)
            {
                List<FullCourse> courses = await new MoodleRepository().GetEnrolledCourses();

                List<Lesson> matchedLessons = lessons
                    .Where(l => courses.SingleOrDefault(c => 
                        c.ShortName.Contains($":{l.ShortName}:")
                        || l.FullName.Normalize().StartsWith(c.FullName.Normalize())) != null)
                    .ToList();

                return matchedLessons;
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
                await Shell.Current.GoToAsync("..", true);
            }
        }
    }
}