using NureTimetable.DAL;
using NureTimetable.Models;
using NureTimetable.Models.Consts;
using NureTimetable.ViewModels;
using Syncfusion.SfSchedule.XForms;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NureTimetable.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class TimetablePage : ContentPage
    {
        public EventList events { get; set; }

        public TimetablePage ()
		{
			InitializeComponent ();

            UpdateEvents(GroupsDataStore.GetSelected());
            MessagingCenter.Subscribe<Application, Group>(this, MessageTypes.SelectedGroupChanged, (sender, newSelectedGroup) =>
            {
                UpdateEvents(newSelectedGroup);
            });
            MessagingCenter.Subscribe<Application, int>(this, MessageTypes.TimetableUpdated, (sender, groupID) =>
            {
                Group selectedGroup = GroupsDataStore.GetSelected();
                if (selectedGroup == null || selectedGroup.ID != groupID)
                {
                    return;
                }
                UpdateEvents(selectedGroup);
            });
            MessagingCenter.Subscribe<Application, int>(this, MessageTypes.LessonSettingsChanged, (sender, groupID) =>
            {
                Group selectedGroup = GroupsDataStore.GetSelected();
                if (selectedGroup == null || selectedGroup.ID != groupID)
                {
                    return;
                }
                UpdateEvents(selectedGroup);
            });
        }

        private void UpdateEvents(Group selectedGroup)
        {
            if (selectedGroup == null)
            {
                TimetableLayout.IsVisible = false;
                NoSourceLayout.IsVisible = true;
                return;
            }
            else
            {
                NoSourceLayout.IsVisible = false;
                TimetableLayout.IsVisible = true;
            }

            GroupName.Text = selectedGroup.Name;
            events = EventsDataStore.GetEvents(selectedGroup.ID);

            if (events == null || events.Count == 0)
            {
                Timetable.DataSource = null;
                return;
            }

            List<LessonSettings> lessonSettings = LessonSettingsDataStore.GetLessonSettings(selectedGroup.ID)
                .Where(ls => ls.IsSomeSettingsApplied)
                .ToList();
            for (int i = 0; i < events.Events.Count; i++)
            {
                LessonSettings lessonSetting = lessonSettings.FirstOrDefault(ls => ls.LessonName == events.Events[i].Lesson);
                if (lessonSetting == null)
                {
                    continue;
                }
                if (lessonSetting.HidingSettings.HideLesson)
                {
                    events.Events.RemoveAt(i);
                    i--;
                    continue;
                }
            }

            Timetable.MinDisplayDate = events.StartDate();
            Timetable.MaxDisplayDate = events.EndDate();
            Timetable.WeekViewSettings.WorkStartHour = events.StartTime().TotalHours;
            Timetable.WeekViewSettings.WorkEndHour = events.EndTime().TotalHours;
            Timetable.DataSource = events.Events;
        }

        private void ManageGroups_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new ManageGroupsPage());
        }

        private void Today_Clicked(object sender, EventArgs e)
        {
            if (!TimetableLayout.IsVisible)
            {
                return;
            }
            Timetable.NavigateTo(DateTime.Now);
        }

        private void Timetable_CellTapped(object sender, CellTappedEventArgs e)
        {
            Event ev = (Event)e.Appointment;
            if (ev == null)
            {
                return;
            }
            string nl = Environment.NewLine;
            DisplayAlert($"{ev.Lesson} - {ev.Type}", $"Аудитория: {ev.Room}{nl}День: {ev.Start.ToString("ddd, dd.MM.yy")}{nl}Время: {ev.Start.ToString("HH:mm")} - {ev.End.ToString("HH:mm")}", "Ok");
        }
    }
}