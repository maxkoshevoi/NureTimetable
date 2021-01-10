using NureTimetable.Core.Models.Settings;
using NureTimetable.DAL;

namespace NureTimetable.Migrations
{
    class RemoveTimelineViewMode : BaseMigration
    {
        protected override bool IsNeedsToBeAppliedInternal()
        {
            return (int)SettingsRepository.Settings.TimetableViewMode == 3;
        }

        protected override bool ApplyInternal()
        {
            SettingsRepository.Settings.TimetableViewMode = TimetableViewMode.Week;
            return true;
        }
    }
}
