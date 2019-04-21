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

        public LessonSettingsPage (LessonInfo lessonInfo, List<EventType> eventTypes)
		{
			InitializeComponent ();
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
            LessonsEventTypes.ItemsSource = this.eventTypes;
            UpdateEventTypesCheck();
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
            }
            else
            {
                foreach (CheckedEventType eventType in eventTypes)
                {
                    eventType.IsChecked = !lessonInfo.Settings.Hiding.HideOnlyThisEventTypes.Contains(eventType.EventType.ID);
                    eventType.NotifyChanged();
                }
            }
            updatingProgrammatically = false;
        }

        private void EventType_StateChanged(object sender, StateChangedEventArgs e)
        {
            CheckedEventType eventType = (CheckedEventType)((SfCheckBox)sender).BindingContext;
            lessonInfo.Settings.Hiding.HideOnlyThisEventTypes.RemoveAll(id => id == eventType.EventType.ID);
            if (e.IsChecked == false)
            {
                lessonInfo.Settings.Hiding.HideOnlyThisEventTypes.Add(eventType.EventType.ID);
            }
            UpdateShowLessonCheck();
        }

        private void UpdateShowLessonCheck()
        {
            if (updatingProgrammatically) return;

            updatingProgrammatically = true;
            if (lessonInfo.Settings.Hiding.HideOnlyThisEventTypes.Count == 0)
            {
                ShowLesson.IsChecked = true;
            }
            else if (lessonInfo.Settings.Hiding.HideOnlyThisEventTypes.Count == eventTypes.Count)
            {
                ShowLesson.IsChecked = false;
            }
            else
            {
                ShowLesson.IsChecked = null;
            }
            updatingProgrammatically = false;
        }

        private void LessonsEventTypes_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            ((ListView)sender).SelectedItem = null;
        }

        private void LessonNotes_TextChanged(object sender, TextChangedEventArgs e)
        {
            lessonInfo.Notes = LessonNotes.Text;
        }
    }
}