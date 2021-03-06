﻿using NureTimetable.Core.Models.Consts;
using NureTimetable.DAL.Models.Local;
using NureTimetable.UI.Views;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace NureTimetable.UI.ViewModels
{
    public class LessonViewModel : BaseViewModel
    {
        private readonly TimetableInfo timetableInfo;
        private readonly ManageLessonsViewModel manageLessonsViewModel;

        #region Properties
        private LessonInfo _lessonInfo;
        public LessonInfo LessonInfo { get => _lessonInfo; set { _lessonInfo = value; OnPropertyChanged(nameof(IsChecked)); } }

        public bool? IsChecked 
        { 
            get => LessonInfo.Settings.Hiding.ShowLesson;
            set
            {
                if (IsChecked == value)
                    return;

                LessonInfo.Settings.Hiding.ShowLesson = value;
                OnPropertyChanged();
                manageLessonsViewModel.HasUnsavedChanges = true;
            } 
        }

        public IAsyncCommand SettingsClickedCommand { get; }
        public IAsyncCommand InfoClickedCommand { get; }
        #endregion

        public LessonViewModel(LessonInfo lessonInfo, TimetableInfo timetableInfo, ManageLessonsViewModel manageLessonsViewModel)
        {
            this._lessonInfo = lessonInfo;
            this.timetableInfo = timetableInfo;
            this.manageLessonsViewModel = manageLessonsViewModel;

            MessagingCenter.Subscribe<LessonSettingsViewModel, LessonInfo>(this, MessageTypes.OneLessonSettingsChanged, (sender, newLessonSettings) =>
            {
                if (LessonInfo.Lesson != newLessonSettings.Lesson)
                    return;

                LessonInfo = newLessonSettings;
                manageLessonsViewModel.HasUnsavedChanges = true;
            });

            SettingsClickedCommand = CommandFactory.Create(SettingsClicked, allowsMultipleExecutions: false);
            InfoClickedCommand = CommandFactory.Create(InfoClicked, allowsMultipleExecutions: false);
        }

        private Task SettingsClicked() => 
            Navigation.PushAsync(new LessonSettingsPage
            {
                BindingContext = new LessonSettingsViewModel(LessonInfo, timetableInfo, false)
            });

        private Task InfoClicked() => 
            Navigation.PushAsync(new LessonInfoPage
            {
                BindingContext = new LessonInfoViewModel(LessonInfo, timetableInfo)
            });
    }
}
