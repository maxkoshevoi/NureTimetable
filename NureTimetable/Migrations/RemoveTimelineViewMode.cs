using NureTimetable.DAL.Settings;
using NureTimetable.DAL.Settings.Models;

namespace NureTimetable.Migrations;

class RemoveTimelineViewMode : BaseMigration
{
    protected override Task<bool> IsNeedsToBeAppliedInternal()
    {
        return Task.FromResult((int)SettingsRepository.Settings.TimetableViewMode == 3);
    }

    protected override Task<bool> ApplyInternal()
    {
        SettingsRepository.Settings.TimetableViewMode = TimetableViewMode.Week;
        return Task.FromResult(true);
    }
}
