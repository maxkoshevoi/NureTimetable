using NureTimetable.Core.Localization;
using NureTimetable.Core.Models;
using NureTimetable.Core.Models.Consts;
using NureTimetable.Core.Models.InterplatformCommunication;
using NureTimetable.DAL;
using NureTimetable.DAL.Models.Local;
using NureTimetable.Models;
using NureTimetable.Models.Consts.Fonts;
using NureTimetable.UI.Views.TimetableEntities;
using NureTimetable.ViewModels.TimetableEntities;
using Syncfusion.SfSchedule.XForms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NureTimetable.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TimetablePage : ContentPage
    {
        public TimetableInfoList timetableInfoList { get; set; } = null;
        public bool ApplyHiddingSettings = true;

        private bool isFirstLoad = true;
        private bool isPageVisible = false;
        private List<DateTime> visibleDates = new List<DateTime>();
        private object enumeratingEvents = new object();
        bool lastTimeLeftVisible;

        public TimetablePage()
        {
            InitializeComponent();
            AppSettings settings = SettingsRepository.GetSettings();

            lastTimeLeftVisible = TimeLeft.IsVisible;
            Timetable.VerticalOptions = LayoutOptions.FillAndExpand;
            Timetable.VisibleDatesChangedEvent += Timetable_VisibleDatesChangedEvent;
            string activeCultureCode = Cultures.SupportedCultures[0].TwoLetterISOLanguageName;
            if (Cultures.SupportedCultures.Any(c => c.TwoLetterISOLanguageName == CultureInfo.CurrentCulture.TwoLetterISOLanguageName))
            {
                activeCultureCode = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            }
            Timetable.Locale = activeCultureCode;
            switch (settings.TimetableViewMode)
            {
                case TimetableViewMode.Day:
                    Timetable.ScheduleView = ScheduleView.DayView;
                    break;
                case TimetableViewMode.Week:
                    Timetable.ScheduleView = ScheduleView.WeekView;
                    break;
                case TimetableViewMode.Month:
                    Timetable.ScheduleView = ScheduleView.MonthView;
                    break;
            }

            MessagingCenter.Subscribe<Application, List<SavedEntity>>(this, MessageTypes.SelectedEntitiesChanged, (sender, newSelectedEntities) =>
            {
                UpdateEvents(newSelectedEntities);
            });
            MessagingCenter.Subscribe<Application, SavedEntity>(this, MessageTypes.TimetableUpdated, (sender, entity) =>
            {
                List<SavedEntity> selectedEntities = UniversityEntitiesRepository.GetSelected();
                if (selectedEntities == null || !selectedEntities.Contains(entity))
                {
                    return;
                }
                UpdateEvents(selectedEntities);
            });
            MessagingCenter.Subscribe<Application, SavedEntity>(this, MessageTypes.LessonSettingsChanged, (sender, entity) =>
            {
                List<SavedEntity> selectedEntities = UniversityEntitiesRepository.GetSelected();
                if (selectedEntities.Count == 0 || !selectedEntities.Contains(entity))
                {
                    return;
                }
                UpdateEvents(selectedEntities);
            });
        }

        async void Timetable_VisibleDatesChangedEvent(object sender, VisibleDatesChangedEventArgs e)
        {
            await UpdateTodayButton(false, e);
        }

        private async Task UpdateTodayButton(bool isForceUpdate, VisibleDatesChangedEventArgs visibleDatesChangedArgs = null)
        {
            if (visibleDatesChangedArgs != null)
            {
                visibleDates = visibleDatesChangedArgs.visibleDates;
            }
            if (visibleDates.Count == 0)
            {
                return;
            }

            // Updating Today button
            if (visibleDates.Any(d => d.Date == DateTime.Now.Date))
            {
                if (BToday.Scale == 1)
                {
                    await BToday.ScaleTo(0, 250);
                }
            }
            else if (isForceUpdate || BToday.Scale == 0)
            {
                if (visibleDates[0].Date > DateTime.Now)
                {
                    BToday.Text = MaterialFont.ChevronLeft;
                }
                else
                {
                    BToday.Text = MaterialFont.ChevronRight;
                }
                await BToday.ScaleTo(1, 250);
            }
        }

        private async void ContentPage_Appearing(object sender, EventArgs e)
        {
            isPageVisible = true;

            if (isFirstLoad)
            {
                isFirstLoad = false;

                UpdateEventsWithUI();
            }

            UpdateTimeLeft();
            await UpdateTodayButton(true);
            Device.StartTimer(TimeSpan.FromSeconds(1), UpdateTimeLeft);
        }

        private void ContentPage_Disappearing(object sender, EventArgs e)
        {
            isPageVisible = false;
        }

        private bool UpdateTimeLeft()
        {
            if (timetableInfoList != null && timetableInfoList.Count > 0)
            {
                string text = null;
                lock (enumeratingEvents)
                {
                    Event currentEvent = timetableInfoList.Events.FirstOrDefault(e => e.Start <= DateTime.Now && e.End >= DateTime.Now);
                    if (currentEvent != null)
                    {
                        text = string.Format(LN.TimeUntilBreak, (currentEvent.End - DateTime.Now).ToString("hh\\:mm\\:ss"));
                    }
                    else
                    {
                        Event nextEvent = timetableInfoList.Events
                            .Where(e => e.Start > DateTime.Now)
                            .OrderBy(e => e.Start)
                            .FirstOrDefault();
                        if (nextEvent != null && nextEvent.Start.Date == DateTime.Now.Date)
                        {
                            text = string.Format(
                                LN.TimeUntilLesson, 
                                nextEvent.Lesson.ShortName, 
                                nextEvent.RoomName, 
                                (nextEvent.Start - DateTime.Now).ToString("hh\\:mm\\:ss")
                            );
                        }
                    }
                }
                if (string.IsNullOrEmpty(text) || !isPageVisible)
                {
                    TimeLeft.Text = null;
                    if (string.IsNullOrEmpty(text) && TimeLeft.IsVisible)
                    {
                        TimeLeft.IsVisible = false;
                        UpdateTimetableHeight();
                    }
                }
                else
                {
                    TimeLeft.Text = text;
                    TimeLeft.IsVisible = true;
                }

                if (TimeLeft.IsVisible != lastTimeLeftVisible && TimeLeft.Height > 0)
                {
                    UpdateTimetableHeight();
                    lastTimeLeftVisible = TimeLeft.IsVisible;
                }
            }
            return isPageVisible;
        }
        
        private void UpdateEventsWithUI()
        {
            Timetable.IsEnabled = false;
            ProgressLayout.IsVisible = true;

            Task.Factory.StartNew(() =>
            {
                UpdateEvents(UniversityEntitiesRepository.GetSelected());

                Device.BeginInvokeOnMainThread(() =>
                {
                    ProgressLayout.IsVisible = false;
                    Timetable.IsEnabled = true;
                });
            });
        }

        private void UpdateEvents(List<SavedEntity> selectedEntities)
        {
            if (selectedEntities == null || selectedEntities.Count == 0)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    Title = LN.AppName;
                });
                TimetableLayout.IsVisible = false;
                NoSourceLayout.IsVisible = true;
                return;
            }
            else
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    Title = string.Join(", ", selectedEntities.Select(se => se.Name));
                });
                NoSourceLayout.IsVisible = false;
                TimetableLayout.IsVisible = true;
            }

            lock (enumeratingEvents)
            {
                var timetableInfos = new List<TimetableInfo>();
                foreach (SavedEntity entity in selectedEntities)
                {
                    TimetableInfo timetableInfo = EventsRepository.GetEvents(entity);
                    if (timetableInfo != null)
                    {
                        timetableInfos.Add(timetableInfo);
                    }
                }
                timetableInfoList = TimetableInfoList.Build(timetableInfos, ApplyHiddingSettings);
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                if (timetableInfoList.Count == 0)
                {
                    Timetable.DataSource = null;
                }
                else
                {
                    int retriesLeft = 25;
                    Exception exception = null;
                    do
                    {
                        try
                        {
                            lock (enumeratingEvents)
                            {
                                Timetable.MinDisplayDate = timetableInfoList.StartDate();
                                Timetable.MaxDisplayDate = timetableInfoList.EndDate();

                                Timetable.WeekViewSettings.StartHour = 0;
                                Timetable.WeekViewSettings.EndHour = 24;
                                Timetable.WeekViewSettings.StartHour = timetableInfoList.StartTime().TotalHours;
                                Timetable.WeekViewSettings.EndHour = timetableInfoList.EndTime().TotalHours + (Timetable.TimeInterval / 60 / 2);

                                Timetable.DayViewSettings.StartHour = 0;
                                Timetable.DayViewSettings.EndHour = 24;
                                Timetable.DayViewSettings.StartHour = timetableInfoList.StartTime().TotalHours;
                                Timetable.DayViewSettings.EndHour = timetableInfoList.EndTime().TotalHours + (Timetable.TimeInterval / 60 / 2);
                            }

                            UpdateTimetableHeight();

                            // Fix for bug when view header isn`t updating on first swipe
                            try
                            {
                                Timetable.VisibleDatesChangedEvent -= Timetable_VisibleDatesChangedEvent;

                                DateTime currebtDate = visibleDates.Count > 0 ? visibleDates[0] : DateTime.Now;
                                Timetable.NavigateTo(currebtDate.AddDays(7));
                                Timetable.NavigateTo(currebtDate);
                            }
                            catch { }
                            finally
                            {
                                Timetable.VisibleDatesChangedEvent += Timetable_VisibleDatesChangedEvent;
                            }

                            Timetable.DataSource = timetableInfoList.Events.Select(ev => new EventViewModel(ev)).ToList();

                            UpdateTimeLeft();
                            break;
                        }
                        catch (Exception ex)
                        {
                            // Potential error with the SfSchedule control. Needs investigation!
                            exception = ex;
                        }

                        retriesLeft--;
                    } while (retriesLeft > 0);
                    if (retriesLeft == 0 && exception != null)
                    {
                        MessagingCenter.Send(Application.Current, MessageTypes.ExceptionOccurred, exception);

                        DisplayAlert(LN.TimetableDisplay, LN.TimetableDisplayError, LN.Ok);
                        return;
                    }
                }
            });
        }

        private void ContentPage_SizeChanged(object sender, EventArgs e)
        {
            UpdateTimetableHeight();
        }

        private void UpdateTimetableHeight()
        {
            if (Timetable.Height <= 0 || timetableInfoList == null || timetableInfoList.Count == 0) return;

            Timetable.VerticalOptions = LayoutOptions.Fill;
            Timetable.HeightRequest = TimetableLayout.Height - (TimeLeft.IsVisible ? TimeLeft.Height + TimeLeft.Margin.VerticalThickness : 0);
            double timeIntrvalsCount = (Timetable.WeekViewSettings.EndHour - Timetable.WeekViewSettings.StartHour) / (Timetable.TimeInterval / 60);
            double magicNumberToMakeMathWork = 1.57;
            double timeIntervalHeightToFit = (Timetable.HeightRequest - Timetable.HeaderHeight - Timetable.ViewHeaderHeight)*magicNumberToMakeMathWork / (timeIntrvalsCount/* + 1*/);
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
            Navigation.PushAsync(new ManageEntitiesPage()
            {
                BindingContext = new ManageEntitiesViewModel(Navigation)
            });
        }

        private async void Today_Clicked(object sender, EventArgs e)
        {
            if (!TimetableLayout.IsVisible)
            {
                return;
            }

            string displayMode = await DisplayActionSheet(LN.ChooseDisplayMode, LN.Cancel, null, LN.Day, LN.Week, LN.Month);

            AppSettings settings = SettingsRepository.GetSettings();
            DateTime? selected = Timetable.SelectedDate;
            Timetable.SelectedDate = null;
            if (displayMode == LN.Day)
            {
                Timetable.ScheduleView = ScheduleView.DayView;
                settings.TimetableViewMode = TimetableViewMode.Day;
            }
            else if (displayMode == LN.Week)
            {
                Timetable.ScheduleView = ScheduleView.WeekView;
                settings.TimetableViewMode = TimetableViewMode.Week;
            }
            else if (displayMode == LN.Month)
            {
                Timetable.ScheduleView = ScheduleView.MonthView;
                settings.TimetableViewMode = TimetableViewMode.Month;
            }
            if (selected != null)
            {
                Timetable.NavigateTo(selected.Value);
                //Timetable.SelectedDate = selected;
            }

            SettingsRepository.UpdateSettings(settings);
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

            LessonInfo lessonInfo = timetableInfoList.LessonsInfo?.FirstOrDefault(li => li.Lesson == ev.Lesson);
            string notes = null;
            if (lessonInfo != null)
            {
                if (!string.IsNullOrEmpty(lessonInfo.Notes))
                {
                    notes = nl + nl + lessonInfo.Notes;
                }
            }
            DisplayAlert($"{ev.Lesson.FullName}", string.Format(LN.EventType, ev.Type.FullName) + nl +
                string.Format(LN.EventClassroom, ev.RoomName) + nl +
                string.Format(LN.EventTeachers, string.Join(", ", ev.Teachers.Select(t => t.Name))) + nl +
                string.Format(LN.EventGroups, string.Join(", ", ev.Groups.Select(t => t.Name))) + nl +
                string.Format(LN.EventDay, ev.Start.ToString("ddd, dd.MM.yy")) + nl +
                string.Format(LN.EventTime, ev.Start.ToString("HH:mm"), ev.End.ToString("HH:mm")) + 
                notes, LN.Ok);
        }

        private void HideSelectedEvents_Clicked(object sender, EventArgs e)
        {
            if (!TimetableLayout.IsVisible)
            {
                return;
            }

            ApplyHiddingSettings = !ApplyHiddingSettings;

            string message, icon;
            if (ApplyHiddingSettings)
            {
                icon = "bookmark_border";
                message = LN.SelectedEventsShown;
            }
            else
            {
                icon = "bookmark";
                message = LN.AllEventsShown;
            }
            HideSelectedEvents.IconImageSource = icon;
            DependencyService.Get<IMessage>().LongAlert(message);

            UpdateEventsWithUI();
        }

        void BToday_Clicked(object sender, EventArgs e)
        {
            DateTime today = DateTime.Now.Date;

            DateTime moveTo = today;
            if (Timetable.MinDisplayDate > moveTo)
            {
                moveTo = Timetable.MinDisplayDate;
            }
            else if (Timetable.MaxDisplayDate < moveTo)
            {
                moveTo = Timetable.MaxDisplayDate;
            }
            if (moveTo != today && visibleDates.Contains(moveTo))
            {
                DisplayAlert(LN.ShowToday, LN.NoTodayTimetable, LN.Ok);
                return;
            }

            Timetable.NavigateTo(moveTo);
        }
    }
}