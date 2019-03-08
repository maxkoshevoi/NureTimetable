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
        LessonInfo lessonInfo;
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

        public LessonSettingsPage (LessonInfo lessonInfo)
		{
			InitializeComponent ();
            Title = lessonInfo.LongName;

            this.lessonInfo = lessonInfo;
            updatingProgrammatically = true;
            ShowLesson.IsChecked = lessonInfo.Settings.Hiding.ShowLesson;
            LessonNotes.Text = lessonInfo.Notes;
            updatingProgrammatically = false;

            eventTypes = lessonInfo.EventTypesInfo.Select(et => et.Name)
                .Select(et => new CheckedEventType
                {
                    EventType = et
                })
                .OrderBy(et => et.EventType)
                .ToList();
            LessonsEventTypes.ItemsSource = eventTypes;
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
                    eventType.IsChecked = !lessonInfo.Settings.Hiding.HideOnlyThisEventTypes.Contains(eventType.EventType);
                    eventType.NotifyChanged();
                }
            }
            updatingProgrammatically = false;
        }

        private void EventType_StateChanged(object sender, StateChangedEventArgs e)
        {
            string name = ((SfCheckBox)sender).Text;
            lessonInfo.Settings.Hiding.HideOnlyThisEventTypes.RemoveAll(item => item == name);
            if (e.IsChecked == false)
            {
                lessonInfo.Settings.Hiding.HideOnlyThisEventTypes.Add(name);
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
            else if (lessonInfo.Settings.Hiding.HideOnlyThisEventTypes.Count == lessonInfo.EventTypesInfo.Count)
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