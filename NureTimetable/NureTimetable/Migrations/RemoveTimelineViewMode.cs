using System.Threading.Tasks;
using NureTimetable.Core.Models.Settings;
using NureTimetable.DAL;

namespace NureTimetable.Migrations
{
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
}
