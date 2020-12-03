using NureTimetable.Core.Models.Settings;
using NureTimetable.DAL;

namespace NureTimetable.Migrations
{
    class RemoveTimelineViewMode : BaseMigration
    {
        public override bool IsNeedsToBeApplied()
        {
            return HandleException(() =>
            {
                return (int)SettingsRepository.Settings.TimetableViewMode == 3;
            });

        }

        public override bool Apply()
        {
            return HandleException(() =>
            {
                SettingsRepository.Settings.TimetableViewMode = TimetableViewMode.Week;
                return true;
            });
        }
    }
}
