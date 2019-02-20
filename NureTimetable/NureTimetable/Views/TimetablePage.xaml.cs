using NureTimetable.DAL;
using NureTimetable.Licalization;
using NureTimetable.Models;
using NureTimetable.Models.Consts;
using NureTimetable.ViewModels;
using Syncfusion.SfSchedule.XForms;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        private bool isFirstLoad = true;
        private List<DateTime> visibleDates = new List<DateTime>();
        public EventList events { get; set; } = null;

        public TimetablePage()
        {
            InitializeComponent();

            Timetable.VisibleDatesChangedEvent += Timetable_VisibleDatesChangedEvent;
            string activeCultureCode = Cultures.SupportedCultures[0].TwoLetterISOLanguageName;
            if (Cultures.SupportedCultures.Any(c => c.TwoLetterISOLanguageName == CultureInfo.CurrentCulture.TwoLetterISOLanguageName))
            {
                activeCultureCode = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            }
            Timetable.Locale = activeCultureCode;

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

        private void Timetable_VisibleDatesChangedEvent(object sender, VisibleDatesChangedEventArgs e)
        {
            visibleDates = e.visibleDates;
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
                            Timetable.WeekViewSettings.StartHour = 0;
                            Timetable.WeekViewSettings.EndHour = 24;
                            Timetable.WeekViewSettings.StartHour = events.StartTime().TotalHours;
                            Timetable.WeekViewSettings.EndHour = events.EndTime().TotalHours;

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

                        DisplayAlert("Отображение расписания", "Произошла ошибка при попытке загрузить расписание", "Повторить попытку", "Ok");
                        return;
                    }

                    // Fix for bug when view header isn`t updating on first swipe
                    DateTime currebtDate = visibleDates.Count > 0 ? visibleDates[0] : DateTime.Now;
                    Timetable.NavigateTo(currebtDate.AddDays(7));
                    Timetable.NavigateTo(currebtDate);

                    Timetable.DataSource = events.Events;
                }
            });
        }

        private void ContentPage_SizeChanged(object sender, EventArgs e)
        {
            UpdateTimetableHeight();
        }

        private void UpdateTimetableHeight()
        {
            if (Timetable.Height <= 0 || events == null || events.Count == 0) return;

            double timeIntrvalsCount = (Timetable.WeekViewSettings.EndHour - Timetable.WeekViewSettings.StartHour) / (Timetable.TimeInterval / 60);
            double magicNumberToMakeMathWork = 1.57;
            double timeIntervalHeightToFit = (Timetable.Height - Timetable.HeaderHeight - Timetable.ViewHeaderHeight)*magicNumberToMakeMathWork / (timeIntrvalsCount/* + 1*/);
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
            //    Timetable.NavigateTo(new DateTime(dateCenter.Date.Ticks + timeCenter.Ticks)); // Potential System.ObjectDisposedException on this line
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

            DateTime? selected = Timetable.SelectedDate;
            Timetable.SelectedDate = null;
            if (Timetable.ScheduleView == ScheduleView.WeekView)
            {
                Timetable.ScheduleView = ScheduleView.MonthView;
            }
            else
            {
                Timetable.ScheduleView = ScheduleView.WeekView;
            }
            if (selected != null)
            {
                Timetable.NavigateTo(selected.Value);
                //Timetable.SelectedDate = selected;
            }
        }

        private void Timetable_CellTapped(object sender, CellTappedEventArgs e)
        {
            DisplayEventDetails((Event)e.Appointment);
        }

        private void Timetable_MonthInlineAppointmentTapped(object sender, MonthInlineAppointmentTappedEventArgs e)
        {
            DisplayEventDetails((Event)e.Appointment);
        }

        private void DisplayEventDetails(Event ev)
        {
            if (ev == null)
            {
                return;
            }
            string nl = Environment.NewLine;
            DisplayAlert($"{ev.Lesson} - {ev.Type}", $"Аудитория: {ev.Room}{nl}День: {ev.Start.ToString("ddd, dd.MM.yy")}{nl}Время: {ev.Start.ToString("HH:mm")} - {ev.End.ToString("HH:mm")}", "Ok");
        }
    }
}