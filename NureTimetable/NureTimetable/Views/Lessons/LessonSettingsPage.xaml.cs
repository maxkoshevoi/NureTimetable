using NureTimetable.DAL;
using NureTimetable.Helpers;
using NureTimetable.Models;
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
            
        public LessonSettingsPage (LessonSettings lessonSettings)
		{
			InitializeComponent ();
            Title = lessonSettings.LessonName;

            this.lessonSettings = lessonSettings;
            SwShowLesson.IsToggled = !lessonSettings.HidingSettings.HideLesson;
        }

        private void SwShowLesson_Toggled(object sender, ToggledEventArgs e)
        {
            lessonSettings.HidingSettings.HideLesson = !SwShowLesson.IsToggled;
            MessagingCenter.Send(this, "OneLessonSettingsChanged", lessonSettings);
        }
    }
}