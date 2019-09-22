using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NureTimetable.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TimetablePage : ContentPage, ITimetablePageCommands
    {
        public TimetablePage()
        {
            InitializeComponent();
            Timetable.VerticalOptions = LayoutOptions.FillAndExpand;
        }

        public void UpdateTimetableHeight()
        {
            if (Timetable.Height <= 0) return;

            Timetable.VerticalOptions = LayoutOptions.Fill;
            Timetable.HeightRequest = TimetableLayout.Height - (TimeLeft.IsVisible ? TimeLeft.Height + TimeLeft.Margin.VerticalThickness : 0);
            double timeIntrvalsCount = (Timetable.WeekViewSettings.EndHour - Timetable.WeekViewSettings.StartHour) / (Timetable.TimeInterval / 60);
            double magicNumberToMakeMathWork = 1.57;
            double timeIntervalHeightToFit = (Timetable.HeightRequest - Timetable.HeaderHeight - Timetable.ViewHeaderHeight) * magicNumberToMakeMathWork / (timeIntrvalsCount/* + 1*/);
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

        public void TimetableNavigateTo(DateTime date)
        {
            Timetable.NavigateTo(date);
        }

        public async Task ScaleTodayButtonTo(double scale)
        {
            await BToday.ScaleTo(scale);
        }
    }
}