using NureTimetable.DAL.Models.Local;
using Syncfusion.XForms.Buttons;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NureTimetable.Views.Lessons
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LessonSettingsPage : ContentPage
    {
        LessonInfo lessonInfo;
        List<CheckedEventType> eventTypes;
        List<CheckedTeacher> teachers;
        bool updatingProgrammatically = false;

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

        public LessonSettingsPage (LessonInfo lessonInfo, List<EventType> eventTypes, List<Teacher> teachers)
        {
            InitializeComponent();
            Title = lessonInfo.Lesson.FullName;

            this.lessonInfo = lessonInfo;
            updatingProgrammatically = true;
            ShowLesson.IsChecked = lessonInfo.Settings.Hiding.ShowLesson;
            LessonNotes.Text = lessonInfo.Notes;
            updatingProgrammatically = false;

            this.eventTypes = eventTypes
                .Select(et => new CheckedEventType
                {
                    EventType = et
                })
                .OrderBy(et => et.EventType.ShortName)
                .ToList();
            ChangeListViewItemSource(LessonEventTypes, this.eventTypes);
            this.teachers = teachers
                .Select(t => new CheckedTeacher
                {
                    Teacher = t
                })
                .OrderBy(et => et.Teacher.ShortName)
                .ToList();
            ChangeListViewItemSource(LessonTeachers, this.teachers);
            UpdateEventTypesCheck();
        }

        private void ChangeListViewItemSource<T>(ListView listView, List<T> newItemsSource)
        {
            const int rowHeight = 43,
                headerHeight = 19;

            listView.ItemsSource = newItemsSource;
            // ListView doesn't support AutoSize, so we have to do it manually
            listView.HeightRequest = rowHeight * newItemsSource.Count + headerHeight;
        }

        private void ShowLesson_StateChanged(object sender, StateChangedEventArgs e)
        {
            lessonInfo.Settings.Hiding.ShowLesson = ShowLesson.IsChecked;
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

        private void EventType_StateChanged(object sender, StateChangedEventArgs e)
        {
            CheckedEventType eventType = (CheckedEventType)((SfCheckBox)sender).BindingContext;
            lessonInfo.Settings.Hiding.EventTypesToHide.RemoveAll(id => id == eventType.EventType.ID);
            if (e.IsChecked == false)
            {
                lessonInfo.Settings.Hiding.EventTypesToHide.Add(eventType.EventType.ID);
            }
            UpdateShowLessonCheck();
        }
        
        private void Teacher_StateChanged(object sender, StateChangedEventArgs e)
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
                ShowLesson.IsChecked = true;
            }
            else if (lessonInfo.Settings.Hiding.EventTypesToHide.Count == eventTypes.Count && lessonInfo.Settings.Hiding.TeachersToHide.Count == teachers.Count)
            {
                ShowLesson.IsChecked = false;
            }
            else
            {
                ShowLesson.IsChecked = null;
            }
            updatingProgrammatically = false;
        }

        private void LessonEventTypes_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            ((ListView)sender).SelectedItem = null;
        }

        private void LessonTeachers_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            ((ListView)sender).SelectedItem = null;
        }

        private void LessonNotes_TextChanged(object sender, TextChangedEventArgs e)
        {
            lessonInfo.Notes = LessonNotes.Text;
        }
    }
}