using NureTimetable.DAL;
using NureTimetable.Helpers;
using NureTimetable.Models;
using Syncfusion.XForms.Buttons;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NureTimetable.Views.Lessons
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LessonSettingsPage : ContentPage
    {
        LessonSettings lessonSettings;
        List<CheckedEventType> eventTypes;
        bool updatingProgrammatically = false;

        private class CheckedEventType : INotifyPropertyChanged
        {
            public string EventType { get; set; }
            public bool IsChecked { get; set; }

            #region INotifyPropertyChanged
            public event PropertyChangedEventHandler PropertyChanged;
            public void NotifyChanged()
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsChecked)));
            }
            #endregion
        }

        public LessonSettingsPage (LessonSettings lessonSettings)
		{
			InitializeComponent ();
            Title = lessonSettings.LessonName;

            this.lessonSettings = lessonSettings;
            updatingProgrammatically = true;
            ShowLesson.IsChecked = lessonSettings.HidingSettings.ShowLesson;
            updatingProgrammatically = false;

            eventTypes = lessonSettings.EventTypes
                .Select(et => new CheckedEventType
                {
                    EventType = et
                })
                .ToList();
            LessonsEventTypes.ItemsSource = eventTypes;
            UpdateEventTypesCheck();
        }
        
        private void ShowLesson_StateChanged(object sender, StateChangedEventArgs e)
        {
            lessonSettings.HidingSettings.ShowLesson = ShowLesson.IsChecked;
            UpdateEventTypesCheck();
            Device.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Send(this, "OneLessonSettingsChanged", lessonSettings);
            });
        }

        private void UpdateEventTypesCheck()
        {
            if (updatingProgrammatically) return;

            updatingProgrammatically = true;
            if (lessonSettings.HidingSettings.ShowLesson != null)
            {
                foreach (CheckedEventType eventType in eventTypes)
                {
                    eventType.IsChecked = (bool)lessonSettings.HidingSettings.ShowLesson;
                    eventType.NotifyChanged();
                }
            }
            else
            {
                foreach (CheckedEventType eventType in eventTypes)
                {
                    eventType.IsChecked = !lessonSettings.HidingSettings.HideOnlyThisEventTypes.Contains(eventType.EventType);
                    eventType.NotifyChanged();
                }
            }
            updatingProgrammatically = false;
        }

        private void EventType_StateChanged(object sender, StateChangedEventArgs e)
        {
            string name = ((SfCheckBox)sender).Text;
            lessonSettings.HidingSettings.HideOnlyThisEventTypes.RemoveAll(item => item == name);
            if (e.IsChecked == false)
            {
                lessonSettings.HidingSettings.HideOnlyThisEventTypes.Add(name);
            }
            UpdateShowLessonCheck();
        }

        private void UpdateShowLessonCheck()
        {
            if (updatingProgrammatically) return;

            updatingProgrammatically = true;
            if (lessonSettings.HidingSettings.HideOnlyThisEventTypes.Count == 0)
            {
                ShowLesson.IsChecked = true;
            }
            else if (lessonSettings.HidingSettings.HideOnlyThisEventTypes.Count == lessonSettings.EventTypes.Count)
            {
                ShowLesson.IsChecked = false;
            }
            else
            {
                ShowLesson.IsChecked = null;
            }
            updatingProgrammatically = false;
        }
    }
}