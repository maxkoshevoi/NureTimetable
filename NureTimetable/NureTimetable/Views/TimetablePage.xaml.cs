using NureTimetable.DAL;
using NureTimetable.Models;
using NureTimetable.Models.Consts;
using NureTimetable.ViewModels;
using Syncfusion.SfSchedule.XForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NureTimetable.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TimetablePage : ContentPage
    {
        private Size PageSize;
        private bool isFirstLoad = true;
        private volatile EventList _events;
        public EventList events { get => _events; set => _events = value; }

        public TimetablePage()
        {
            InitializeComponent();

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


        private void ContentPage_Appearing(object sender, EventArgs e)
        {
            if (!isFirstLoad) return;
            isFirstLoad = false;

            Timetable.IsEnabled = false;
            ProgressLayout.IsVisible = true;

            Task.Factory.StartNew(() =>
            {
                UpdateEvents(GroupsDataStore.GetSelected());

                Device.BeginInvokeOnMainThread(() =>
                {
                    ProgressLayout.IsVisible = false;
                    Timetable.IsEnabled = true;
                });
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
                Device.BeginInvokeOnMainThread(() =>
                {
                    Timetable.DataSource = null;
                });
                return;
            }

            // Applying lesson settings
            foreach (LessonSettings lSetting in LessonSettingsDataStore.GetLessonSettings(selectedGroup.ID).Where(ls => ls.IsSomeSettingsApplied))
            {
                if (!lSetting.IsSomeSettingsApplied) continue;

                // Hidding settings
                if (lSetting.HidingSettings.ShowLesson == false)
                {
                    events.Events.RemoveAll(ev => ev.Lesson == lSetting.LessonName);
                }
                else if (lSetting.HidingSettings.ShowLesson == null)
                {
                    events.Events.RemoveAll(ev => ev.Lesson == lSetting.LessonName && lSetting.HidingSettings.HideOnlyThisEventTypes.Contains(ev.Type));
                }
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    Timetable.MinDisplayDate = events.StartDate();
                    Timetable.MaxDisplayDate = events.EndDate();
                    Timetable.WeekViewSettings.WorkStartHour = events.StartTime().TotalHours;
                    Timetable.WeekViewSettings.WorkEndHour = events.EndTime().TotalHours;

                    UpdateTimetableHeight();
                }
                catch (Exception ex)
                {
                    // Potential error. Needs investigation!!!
                    MessagingCenter.Send(Application.Current, MessageTypes.ExceptionOccurred, ex);
                }

                Timetable.DataSource = events.Events;
            });
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height); //must be called

            if (PageSize.Width != width || PageSize.Height != height)
            {
                PageSize = new Size(width, height);

                UpdateTimetableHeight();
            }
        }

        private void UpdateTimetableHeight()
        {
            if (Application.Current.MainPage == null || events == null || events.Count == 0) return;

            double timeIntrvalsCount = (Timetable.WeekViewSettings.WorkEndHour - Timetable.WeekViewSettings.WorkStartHour) / (Timetable.TimeInterval / 60);
            double timeIntervalHeightToFit = Application.Current.MainPage.Height / (timeIntrvalsCount + 1);

            if (timeIntervalHeightToFit < 50)
            {
                Timetable.TimeIntervalHeight = 50;
            }
            else
            {
                Timetable.TimeIntervalHeight = timeIntervalHeightToFit;

                // Center 
                DateTime dateTimeCenter = Timetable.SelectedDate ?? DateTime.Now;
                TimeSpan timeCenter = TimeSpan.FromMinutes((Timetable.WeekViewSettings.WorkStartHour * 60) - (Timetable.TimeInterval / 2));
                Timetable.NavigateTo(new DateTime(dateTimeCenter.Date.Ticks + timeCenter.Ticks));
            }
            Timetable.TimeIntervalHeight = Math.Max(50, timeIntervalHeightToFit);
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