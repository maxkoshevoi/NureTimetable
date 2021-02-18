using System;
using System.Threading.Tasks;

namespace NureTimetable.UI.Views
{
    public interface ITimetablePageCommands
    {
        void TimetableNavigateTo(DateTime date);

        Task ScaleTodayButtonTo(double scale);
    }
}