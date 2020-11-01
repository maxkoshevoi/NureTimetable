﻿using NureTimetable.DAL.Models.Local;
using NureTimetable.UI.Helpers;
using NureTimetable.UI.ViewModels.Lessons.LessonSettings;
using NureTimetable.UI.Views.Lessons;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NureTimetable.UI.ViewModels.Lessons.ManageLessons
{
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

        public LessonViewModel(LessonInfo lessonInfo, TimetableInfo timetableInfo)
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
                BindingContext = new LessonSettingsViewModel(LessonInfo, timetableInfo)
            });
        }

        private async Task InfoClicked()
        {
            await Navigation.PushAsync(new LessonInfoPage
            {
                BindingContext = new LessonInfoViewModel(LessonInfo, timetableInfo)
            });
        }
    }
}
