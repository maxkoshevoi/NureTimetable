using NureTimetable.Core.Localization;
using NureTimetable.DAL;
using NureTimetable.DAL.Models.Local;
using NureTimetable.Services.Helpers;
using NureTimetable.ViewModels.Core;
using NureTimetable.ViewModels.Lessons;
using NureTimetable.Views.Lessons;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace NureTimetable.ViewModels.TimetableEntities
{
    public class ManageLessonsViewModel : BaseViewModel
    {
        #region Classes
        public class LessonViewModel : BaseViewModel
        {
            #region Variables
            private readonly TimetableInfo timetableInfo;
            #endregion

            #region Properties
            public LessonInfo LessonInfo { get; set; }

            public ICommand SettingsClickedCommand { get; }

            public ICommand InfoClickedCommand { get; }
            #endregion

            public LessonViewModel(INavigation navigation, LessonInfo lessonInfo, TimetableInfo timetableInfo) : base(navigation)
            {
                LessonInfo = lessonInfo;
                this.timetableInfo = timetableInfo;

                SettingsClickedCommand = CommandHelper.CreateCommand(SettingsClicked);
                InfoClickedCommand = CommandHelper.CreateCommand(InfoClicked);
            }
            
            private async Task SettingsClicked()
            {
                await Navigation.PushAsync(new LessonSettingsPage
                {
                    BindingContext = new LessonSettingsViewModel(Navigation, LessonInfo, timetableInfo)
                });
            }

            private async Task InfoClicked()
            {
                await Navigation.PushAsync(new LessonInfoPage
                {
                    BindingContext = new LessonInfoViewModel(Navigation, LessonInfo, timetableInfo)
                });
            }
        }
        #endregion

        #region Variables
        private bool _isNoSourceLayoutVisable;

        private readonly TimetableInfo timetable;

        private string _title;

        private ObservableCollection<LessonViewModel> _lessons;
        #endregion

        #region Properties
        public string Title
        {
            get => _title;
            private set => SetProperty(ref _title, value);
        }

        public bool IsNoSourceLayoutVisable
        {
            get => _isNoSourceLayoutVisable;
            set => SetProperty(ref _isNoSourceLayoutVisable, value);
        }
        
        public ObservableCollection<LessonViewModel> Lessons { get => _lessons; set => SetProperty(ref _lessons, value); }

        public ICommand PageAppearingCommand { get; }

        public ICommand SaveClickedCommand { get; }
        #endregion

        public ManageLessonsViewModel(INavigation navigation, SavedEntity savedEntity) : base(navigation)
        {
            Title = $"{LN.Lessons}: {savedEntity.Name}";

            timetable = EventsRepository.GetTimetableLocal(savedEntity);
            if (timetable == null)
            {
                return;
            }
            List<LessonInfo> lessonInfo = timetable.LessonsInfo;
            Lessons = new ObservableCollection<LessonViewModel>
            (
                timetable.Lessons()
                    .Select(lesson => lessonInfo.FirstOrDefault(li => li.Lesson == lesson) ?? new LessonInfo { Lesson = lesson })
                    .OrderBy(lesson => !timetable.Events.Where(e => e.Start >= DateTime.Now).Any(e => e.Lesson == lesson.Lesson))
                    .ThenBy(lesson => lesson.Lesson.ShortName)
                    .Select(lesson => new LessonViewModel(Navigation, lesson, timetable))
            );
            IsNoSourceLayoutVisable = Lessons.Count == 0;

            MessagingCenter.Subscribe<LessonSettingsPage, LessonInfo>(this, "OneLessonSettingsChanged", (sender, newLessonSettings) =>
            {
                for (int i = 0; i < Lessons.Count; i++)
                {
                    if (Lessons[i].LessonInfo.Lesson == newLessonSettings.Lesson)
                    {
                        Lessons[i].LessonInfo = newLessonSettings;
                        Lessons[i].LessonInfo.Settings.NotifyChanged();
                        break;
                    }
                }
            });

            PageAppearingCommand = CommandHelper.CreateCommand(PageAppearing);
            SaveClickedCommand = CommandHelper.CreateCommand(SaveClicked);
        }

        private async Task PageAppearing()
        {
            if (Lessons == null)
            {
                await App.Current.MainPage.DisplayAlert(LN.LessonsManagement, LN.AtFirstLoadTimetable, LN.Ok);
                await Navigation.PopAsync();
                return;
            }
        }
        
        private async Task SaveClicked()
        {
            EventsRepository.UpdateLessonsInfo(timetable.Entity, Lessons.Select(l => l.LessonInfo).ToList());
            await App.Current.MainPage.DisplayAlert(LN.SavingSettings, string.Format(LN.EntityLessonSettingsSaved, timetable.Entity.Name), LN.Ok);
            await Navigation.PopAsync();
        }
    }
}