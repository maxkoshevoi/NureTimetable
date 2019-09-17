using System;
using System.Threading.Tasks;

namespace NureTimetable.Views
{
    public interface ITimetablePageCommands
    {
        void UpdateTimetableHeight();

        void TimetableNavigateTo(DateTime date);

        Task ScaleTodayButtonTo(double scale);
    }
}