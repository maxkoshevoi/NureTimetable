using NureTimetable.DataStores;
using NureTimetable.Models;
using NureTimetable.Models.Consts;
using NureTimetable.ViewModels;
using Syncfusion.SfSchedule.XForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
            events = EventsDataStore.GetEvents(selectedGroup.ID) ?? new EventList();
            
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
                if (events.Count == 0)
                {
                    Timetable.DataSource = null;
                }
                else
                {
                    int retriesLest = 25;
                    Exception exception = null;
                    do
                    {
                        try
                        {
                            Timetable.MinDisplayDate = events.StartDate();
                            Timetable.MaxDisplayDate = events.EndDate();
                            Timetable.WeekViewSettings.WorkStartHour = 0;
                            Timetable.WeekViewSettings.WorkEndHour = 24;
                            Timetable.WeekViewSettings.WorkStartHour = events.StartTime().TotalHours;
                            Timetable.WeekViewSettings.WorkEndHour = events.EndTime().TotalHours;

                            UpdateTimetableHeight();
                            break;
                        }
                        catch (Exception ex)
                        {
                            // Potential error with the SfSchedule control. Needs investigation!
                            exception = ex;
                        }

                        retriesLest--;
                    } while (retriesLest > 0);
                    if (retriesLest == 0 && exception != null)
                    {
                        MessagingCenter.Send(Application.Current, MessageTypes.ExceptionOccurred, exception);

                        DisplayAlert("Отоброжение расписания", "Произошла ошибка при попытке загрузить расписание", "Повторить попытку", "Ok");
                        return;
                    }

                    Timetable.DataSource = events.Events;
                }
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
            if (Timetable.Height <= 0 || events == null || events.Count == 0) return;

            double timeIntrvalsCount = (Timetable.WeekViewSettings.WorkEndHour - Timetable.WeekViewSettings.WorkStartHour) / (Timetable.TimeInterval / 60);
            double magicNumberToMakeMathWork = 1.4;
            double timeIntervalHeightToFit = (Timetable.Height - Timetable.HeaderHeight)*magicNumberToMakeMathWork / (timeIntrvalsCount/* + 1*/);
            double minTimeInterval = (50 * Timetable.TimeInterval) / 90; // Each 90 minute interval should be equal or more than 50 in size

            if (timeIntervalHeightToFit <= minTimeInterval)
            {
                timeIntervalHeightToFit = minTimeInterval;
            }
            //else
            //{
            //    // Center 
            //    DateTime dateCenter = Timetable.SelectedDate ?? DateTime.Now;
            //    TimeSpan timeCenter = TimeSpan.FromMinutes((Timetable.WeekViewSettings.WorkStartHour * 60) - (Timetable.TimeInterval / 2));
            //    Timetable.NavigateTo(new DateTime(dateCenter.Date.Ticks + timeCenter.Ticks));
            //}
            Timetable.TimeIntervalHeight = timeIntervalHeightToFit;
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