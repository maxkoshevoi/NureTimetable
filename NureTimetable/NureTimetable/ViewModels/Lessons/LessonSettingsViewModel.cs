using NureTimetable.DAL.Models.Local;
using NureTimetable.Services.Helpers;
using NureTimetable.ViewModels.Core;
using Syncfusion.XForms.Buttons;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace NureTimetable.ViewModels.Lessons
{
    public class LessonSettingsViewModel : BaseViewModel
    {
        #region Classes
        private class CheckedEventType : INotifyPropertyChanged
        {
            public EventType EventType { get; set; }

            public bool IsChecked { get; set; }

            #region INotifyPropertyChanged
            public event PropertyChangedEventHandler PropertyChanged;
            public void NotifyChanged()
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsChecked)));
            }
            #endregion
        }

        private class CheckedTeacher : INotifyPropertyChanged
        {
            public Teacher Teacher { get; set; }

            public bool IsChecked { get; set; }

            #region INotifyPropertyChanged
            public event PropertyChangedEventHandler PropertyChanged;
            public void NotifyChanged()
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsChecked)));
            }
            #endregion
        }
        #endregion

        #region Variables
        LessonInfo lessonInfo;
        List<CheckedEventType> eventTypes;
        List<CheckedTeacher> teachers;
        bool updatingProgrammatically = false;

        private bool? _showLessonIsChecked = false;
        private string _lessonNotesText;
        private string _title;
        #endregion

        #region Properties
        public bool? ShowLessonIsChecked { get => _showLessonIsChecked; set => SetProperty(ref _showLessonIsChecked, value); }
        public string LessonNotesText { get => _lessonNotesText; set => SetProperty(ref _lessonNotesText, value); }
        public string Title { get => _title; set => SetProperty(ref _title, value); }

        public ICommand ShowLessonStateChangedCommand;
        public ICommand LessonNotesTextChangedCommand;
        public ICommand EventTypeStateChangedCommand;
        public ICommand TeacherStateChangedCommand;
        #endregion

        public LessonSettingsViewModel(INavigation navigation, LessonInfo lessonInfo, TimetableInfo timetableInfo) : base(navigation)
        {
            Title = lessonInfo.Lesson.FullName;

            this.lessonInfo = lessonInfo;
            updatingProgrammatically = true;
            ShowLessonIsChecked = lessonInfo.Settings.Hiding.ShowLesson;
            LessonNotesText = lessonInfo.Notes;
            updatingProgrammatically = false;

            this.eventTypes = timetableInfo.EventTypes(lessonInfo.Lesson.ID)
                .Select(et => new CheckedEventType
                {
                    EventType = et
                })
                .OrderBy(et => et.EventType.ShortName)
                .ToList();
            ChangeListViewItemSource(LessonEventTypes, this.eventTypes);
            this.teachers = timetableInfo.Teachers(lessonInfo.Lesson.ID)
                .Select(t => new CheckedTeacher
                {
                    Teacher = t
                })
                .OrderBy(et => et.Teacher.ShortName)
                .ToList();
            ChangeListViewItemSource(LessonTeachers, this.teachers);

            UpdateEventTypesCheck();

            ShowLessonStateChangedCommand = CommandHelper.CreateCommand<StateChangedEventArgs>(ShowLessonStateChanged);
            LessonNotesTextChangedCommand = CommandHelper.CreateCommand<TextChangedEventArgs>(LessonNotesTextChanged);
            EventTypeStateChangedCommand = CommandHelper.CreateCommand<object, StateChangedEventArgs>(EventTypeStateChanged);
            TeacherStateChangedCommand = CommandHelper.CreateCommand<object, StateChangedEventArgs>(TeacherStateChanged);

        }

        private void ChangeListViewItemSource<T>(ListView listView, List<T> newItemsSource)
        {
            const int rowHeight = 43,
                headerHeight = 19;

            if (newItemsSource == null || newItemsSource.Count == 0)
            {
                listView.IsVisible = false;
            }
            else
            {
                listView.ItemsSource = newItemsSource;
                // ListView doesn't support AutoSize, so we have to do it manually
                listView.HeightRequest = rowHeight * newItemsSource.Count + headerHeight;
                listView.IsVisible = true;
            }
        }

        private async Task ShowLessonStateChanged(StateChangedEventArgs e)
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
                foreach (CheckedEventType eventType in eventTypes)
                {
                    eventType.IsChecked = (bool)lessonInfo.Settings.Hiding.ShowLesson;
                    eventType.NotifyChanged();
                }
                foreach (CheckedTeacher teacher in teachers)
                {
                    teacher.IsChecked = (bool)lessonInfo.Settings.Hiding.ShowLesson;
                    teacher.NotifyChanged();
                }
            }
            else
            {
                foreach (CheckedEventType eventType in eventTypes)
                {
                    eventType.IsChecked = !lessonInfo.Settings.Hiding.EventTypesToHide.Contains(eventType.EventType.ID);
                    eventType.NotifyChanged();
                }
                foreach (CheckedTeacher teacher in teachers)
                {
                    teacher.IsChecked = !lessonInfo.Settings.Hiding.TeachersToHide.Contains(teacher.Teacher.ID);
                    teacher.NotifyChanged();
                }
            }
            updatingProgrammatically = false;
        }

        private async Task EventTypeStateChanged(object sender, StateChangedEventArgs e)
        {
            CheckedEventType eventType = (CheckedEventType)((SfCheckBox)sender).BindingContext;
            lessonInfo.Settings.Hiding.EventTypesToHide.RemoveAll(id => id == eventType.EventType.ID);
            if (e.IsChecked == false)
            {
                lessonInfo.Settings.Hiding.EventTypesToHide.Add(eventType.EventType.ID);
            }
            UpdateShowLessonCheck();
        }

        private async Task TeacherStateChanged(object sender, StateChangedEventArgs e)
        {
            CheckedTeacher eventType = (CheckedTeacher)((SfCheckBox)sender).BindingContext;
            lessonInfo.Settings.Hiding.TeachersToHide.RemoveAll(id => id == eventType.Teacher.ID);
            if (e.IsChecked == false)
            {
                lessonInfo.Settings.Hiding.TeachersToHide.Add(eventType.Teacher.ID);
            }
            UpdateShowLessonCheck();
        }

        private void UpdateShowLessonCheck()
        {
            if (updatingProgrammatically) return;

            updatingProgrammatically = true;
            if (lessonInfo.Settings.Hiding.EventTypesToHide.Count == 0 && lessonInfo.Settings.Hiding.TeachersToHide.Count == 0)
            {
                ShowLessonIsChecked = true;
            }
            else if (lessonInfo.Settings.Hiding.EventTypesToHide.Count == eventTypes.Count && lessonInfo.Settings.Hiding.TeachersToHide.Count == teachers.Count)
            {
                ShowLessonIsChecked = false;
            }
            else
            {
                ShowLessonIsChecked = null;
            }
            updatingProgrammatically = false;
        }

        private async Task LessonNotesTextChanged(TextChangedEventArgs e)
        {
            lessonInfo.Notes = LessonNotesText;
        }
    }
}
