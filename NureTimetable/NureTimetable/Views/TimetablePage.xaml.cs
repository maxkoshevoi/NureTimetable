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
        public TimetableInfo timetableInfo { get; set; } = null;
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

            MessagingCenter.Subscribe<Application, SavedEntity>(this, MessageTypes.SelectedEntityChanged, (sender, newSelectedEntity) =>
            {
                UpdateEvents(newSelectedEntity);
            });
            MessagingCenter.Subscribe<Application, SavedEntity>(this, MessageTypes.TimetableUpdated, (sender, entity) =>
            {
                SavedEntity selectedEntity = UniversityEntitiesRepository.GetSelected();
                if (selectedEntity == null || selectedEntity != entity)
                {
                    return;
                }
                UpdateEvents(selectedEntity);
            });
            MessagingCenter.Subscribe<Application, SavedEntity>(this, MessageTypes.LessonSettingsChanged, (sender, entity) =>
            {
                SavedEntity selectedEntity = UniversityEntitiesRepository.GetSelected();
                if (selectedEntity == null || selectedEntity != entity)
                {
                    return;
                }
                UpdateEvents(selectedEntity);
            });
        }

        async void Timetable_VisibleDatesChangedEvent(object sender, VisibleDatesChangedEventArgs e)
        {
            visibleDates = e.visibleDates;

            // Updating Today button
            if (visibleDates.Any(d => d.Date == DateTime.Now.Date))
            {
                if (BToday.Scale == 1)
                {
                    await BToday.ScaleTo(0, 250);
                }
            }
            else if (BToday.Scale == 0)
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

        private void ContentPage_Appearing(object sender, EventArgs e)
        {
            isPageVisible = true;

            if (isFirstLoad)
            {
                isFirstLoad = false;

                UpdateEventsWithUI();
            }

            Device.StartTimer(TimeSpan.FromSeconds(1), UpdateTimeLeft);
        }

        private void ContentPage_Disappearing(object sender, EventArgs e)
        {
            isPageVisible = false;
        }

        private bool UpdateTimeLeft()
        {
            if (timetableInfo != null && timetableInfo.Count > 0)
            {
                string text = null;
                lock (enumeratingEvents)
                {
                    Event currentEvent = timetableInfo.Events.FirstOrDefault(e => e.Start <= DateTime.Now && e.End >= DateTime.Now);
                    if (currentEvent != null)
                    {
                        text = $"До перерыва ({currentEvent.RoomName}): {(currentEvent.End - DateTime.Now).ToString("hh\\:mm\\:ss")}";
                    }
                    else
                    {
                        Event nextEvent = timetableInfo.Events
                            .Where(e => e.Start > DateTime.Now)
                            .OrderBy(e => e.Start)
                            .FirstOrDefault();
                        if (nextEvent != null && nextEvent.Start.Date == DateTime.Now.Date)
                        {
                            text = $"До {nextEvent.Lesson.ShortName} ({nextEvent.RoomName}): {(nextEvent.Start - DateTime.Now).ToString("hh\\:mm\\:ss")}";
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

        private void UpdateEvents(SavedEntity selectedEntity)
        {
            if (selectedEntity == null)
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
                    Title = selectedEntity.Name;
                });
                NoSourceLayout.IsVisible = false;
                TimetableLayout.IsVisible = true;
            }

            lock (enumeratingEvents)
            {
                timetableInfo = EventsRepository.GetEvents(selectedEntity) ?? new TimetableInfo();
                if (ApplyHiddingSettings)
                {
                    timetableInfo.ApplyLessonSettings();
                }
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                if (timetableInfo.Count == 0)
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
                                Timetable.MinDisplayDate = timetableInfo.StartDate();
                                Timetable.MaxDisplayDate = timetableInfo.EndDate();

                                Timetable.WeekViewSettings.StartHour = 0;
                                Timetable.WeekViewSettings.EndHour = 24;
                                Timetable.WeekViewSettings.StartHour = timetableInfo.StartTime().TotalHours;
                                Timetable.WeekViewSettings.EndHour = timetableInfo.EndTime().TotalHours + (Timetable.TimeInterval / 60 / 2);

                                Timetable.DayViewSettings.StartHour = 0;
                                Timetable.DayViewSettings.EndHour = 24;
                                Timetable.DayViewSettings.StartHour = timetableInfo.StartTime().TotalHours;
                                Timetable.DayViewSettings.EndHour = timetableInfo.EndTime().TotalHours + (Timetable.TimeInterval / 60 / 2);
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

                            Timetable.DataSource = timetableInfo.Events.Select(ev => new EventViewModel(ev)).ToList();

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

                        DisplayAlert("Отображение расписания", "Произошла ошибка при попытке отобразить расписание", "Ok");
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
            if (Timetable.Height <= 0 || timetableInfo == null || timetableInfo.Count == 0) return;

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
            Navigation.PushAsync(new ManageGroupsPage()
            {
                BindingContext = new ManageGroupsViewModel(Navigation)
            });
        }

        private async void Today_Clicked(object sender, EventArgs e)
        {
            if (!TimetableLayout.IsVisible)
            {
                return;
            }

            string view = await DisplayActionSheet("Выберите вид:", "Отмена", null, "День", "Неделя", "Месяц");

            AppSettings settings = SettingsRepository.GetSettings();
            DateTime? selected = Timetable.SelectedDate;
            Timetable.SelectedDate = null;
            if (view == "День")
            {
                Timetable.ScheduleView = ScheduleView.DayView;
                settings.TimetableViewMode = TimetableViewMode.Day;
            }
            else if (view == "Неделя")
            {
                Timetable.ScheduleView = ScheduleView.WeekView;
                settings.TimetableViewMode = TimetableViewMode.Week;
            }
            else if (view == "Месяц")
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

            LessonInfo lessonInfo = timetableInfo.LessonsInfo.FirstOrDefault(li => li.Lesson == ev.Lesson);
            string notes = null;
            if (lessonInfo != null)
            {
                if (!string.IsNullOrEmpty(lessonInfo.Notes))
                {
                    notes = nl + nl + lessonInfo.Notes;
                }
            }
            DisplayAlert($"{ev.Lesson.FullName}", $"Тип: {ev.Type.FullName}{nl}Аудитория: {ev.RoomName}{nl}Преподаватель: {string.Join(", ", ev.Teachers.Select(t => t.FullName))}{nl}День: {ev.Start.ToString("ddd, dd.MM.yy")}{nl}Время: {ev.Start.ToString("HH:mm")} - {ev.End.ToString("HH:mm")}{notes}", "Ok");
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
                message = "Показаны выбранные события";
            }
            else
            {
                icon = "bookmark";
                message = "Показаны все события";
            }
            HideSelectedEvents.Icon = icon;
            DependencyService.Get<IMessage>().LongAlert(message);

            UpdateEventsWithUI();
        }

        void BToday_Clicked(object sender, EventArgs e)
        {
            Timetable.MoveToDate = DateTime.Now;
        }
    }
}