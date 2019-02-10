using NureTimetable.DAL;
using NureTimetable.Models;
using NureTimetable.Models.Consts;
using NureTimetable.ViewModels;
using System;

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
            events = TimetableDataStore.GetEvents(selectedGroup.ID);

            if (events.Count > 0)
            {
                Timetable.MinDisplayDate = events.StartDate();
                Timetable.MaxDisplayDate = events.EndDate();
                Timetable.DataSource = events.Events;
            }
        }

        private void ManageGroups_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new ManageGroupsPage());
        }

        private void Today_Clicked(object sender, EventArgs e)
        {
            Timetable.NavigateTo(DateTime.Now);
        }
    }
}