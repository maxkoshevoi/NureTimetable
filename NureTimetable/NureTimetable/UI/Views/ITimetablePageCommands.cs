using System;
using System.Threading.Tasks;

namespace NureTimetable.UI.Views
{
    public interface ITimetablePageCommands
    {
        void TimetableNavigateTo(DateTime targetDate);

        Task ScaleTodayButtonTo(double scale);
    }
}