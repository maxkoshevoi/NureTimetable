﻿using NureTimetable.DAL.Models.Local;
using NureTimetable.Services.Helpers;
using NureTimetable.UI.ViewModels.Core;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;

namespace NureTimetable.UI.ViewModels.Lessons.LessonSettings
{
    public class LessonSettingsViewModel : BaseViewModel
    {
        #region Variables
        LessonInfo lessonInfo;
        bool updatingProgrammatically = false;

        private bool? _showLessonIsChecked = false;
        private string _lessonNotesText;
        private string _title;
        #endregion

        #region Properties
        public bool? ShowLessonIsChecked { get => _showLessonIsChecked; set => SetProperty(ref _showLessonIsChecked, value); }
        public string LessonNotesText { get => _lessonNotesText; set => SetProperty(ref _lessonNotesText, value); }
        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public ListViewViewModel<EventType> LvEventTypes { get; set; }
        public ListViewViewModel<Teacher> LvTeachers { get; set; }

        public ICommand ShowLessonStateChangedCommand { get; }
        public ICommand LessonNotesTextChangedCommand { get; }
        #endregion

        public LessonSettingsViewModel(INavigation navigation, LessonInfo lessonInfo, TimetableInfo timetableInfo) : base(navigation)
        {
            Title = lessonInfo.Lesson.FullName;

            this.lessonInfo = lessonInfo;
            updatingProgrammatically = true;
            ShowLessonIsChecked = lessonInfo.Settings.Hiding.ShowLesson;
            LessonNotesText = lessonInfo.Notes;
            updatingProgrammatically = false;

            LvEventTypes = new ListViewViewModel<EventType>(Navigation)
            {
                ItemsSource = new ObservableCollection<CheckedEntity<EventType>>(timetableInfo.EventTypes(lessonInfo.Lesson.ID)
                    .Select(et => new CheckedEntity<EventType>(Navigation, EventTypeStateChanged)
                    {
                        Entity = et
                    })
                    .OrderBy(et => et.Entity.ShortName))
            };
            LvTeachers = new ListViewViewModel<Teacher>(Navigation)
            {
                ItemsSource = new ObservableCollection<CheckedEntity<Teacher>>(timetableInfo.Teachers(lessonInfo.Lesson.ID)
                    .Select(t => new CheckedEntity<Teacher>(Navigation, TeacherStateChanged)
                    {
                        Entity = t,
                    })
                    .OrderBy(et => et.Entity.ShortName))
            };

            UpdateEventTypesCheck();

            ShowLessonStateChangedCommand = CommandHelper.CreateCommand(ShowLessonStateChanged);
            LessonNotesTextChangedCommand = CommandHelper.CreateCommand(LessonNotesTextChanged);

        }
        
        private void EventTypeStateChanged(CheckedEntity<EventType> e)
        {
            lessonInfo.Settings.Hiding.EventTypesToHide.RemoveAll(id => id == e.Entity.ID);
            if (e.IsChecked == false)
            {
                lessonInfo.Settings.Hiding.EventTypesToHide.Add(e.Entity.ID);
            }
            UpdateShowLessonCheck();
        }

        private void TeacherStateChanged(CheckedEntity<Teacher> e)
        {
            lessonInfo.Settings.Hiding.TeachersToHide.RemoveAll(id => id == e.Entity.ID);
            if (e.IsChecked == false)
            {
                lessonInfo.Settings.Hiding.TeachersToHide.Add(e.Entity.ID);
            }
            UpdateShowLessonCheck();
        }

        private void ShowLessonStateChanged()
        {
            lessonInfo.Settings.Hiding.ShowLesson = ShowLessonIsChecked;
            UpdateEventTypesCheck();

            Device.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Send(this, "OneLessonSettingsChanged", lessonInfo);
            });
        }

        private void UpdateEventTypesCheck()
        {
            if (updatingProgrammatically) return;

            updatingProgrammatically = true;
            if (lessonInfo.Settings.Hiding.ShowLesson != null)
            {
                foreach (var eventType in LvEventTypes.ItemsSource)
                {
                    eventType.IsChecked = (bool)lessonInfo.Settings.Hiding.ShowLesson;
                }
                foreach (var teacher in LvTeachers.ItemsSource)
                {
                    teacher.IsChecked = (bool)lessonInfo.Settings.Hiding.ShowLesson;
                }
            }
            else
            {
                foreach (var eventType in LvEventTypes.ItemsSource)
                {
                    eventType.IsChecked = !lessonInfo.Settings.Hiding.EventTypesToHide.Contains(eventType.Entity.ID);
                }
                foreach (var teacher in LvTeachers.ItemsSource)
                {
                    teacher.IsChecked = !lessonInfo.Settings.Hiding.TeachersToHide.Contains(teacher.Entity.ID);
                }
            }
            updatingProgrammatically = false;
        }

        private void UpdateShowLessonCheck()
        {
            if (updatingProgrammatically) return;

            updatingProgrammatically = true;
            if (lessonInfo.Settings.Hiding.EventTypesToHide.Count == 0 && lessonInfo.Settings.Hiding.TeachersToHide.Count == 0)
            {
                ShowLessonIsChecked = true;
            }
            else if (lessonInfo.Settings.Hiding.EventTypesToHide.Count == LvEventTypes.ItemsSource.Count 
                && lessonInfo.Settings.Hiding.TeachersToHide.Count == LvTeachers.ItemsSource.Count)
            {
                ShowLessonIsChecked = false;
            }
            else
            {
                ShowLessonIsChecked = null;
            }
            updatingProgrammatically = false;
        }

        private void LessonNotesTextChanged()
        {
            lessonInfo.Notes = LessonNotesText;
        }
    }
}