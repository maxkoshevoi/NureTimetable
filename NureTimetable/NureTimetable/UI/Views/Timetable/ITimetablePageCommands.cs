namespace NureTimetable.UI.Views
{
    public interface ITimetablePageCommands
    {
        void TimetableNavigateTo(DateTime date);

        Task ScaleTodayButtonTo(double scale);

        Task DisplayToastAsync(string message, int durationMilliseconds = 3000);
    }
}