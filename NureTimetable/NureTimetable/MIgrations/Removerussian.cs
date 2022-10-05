using NureTimetable.DAL.Settings;
using NureTimetable.DAL.Settings.Models;

namespace NureTimetable.Migrations;

class Removerussian : BaseMigration
{
    protected override Task<bool> IsNeedsToBeAppliedInternal()
    {
        return Task.FromResult((int)SettingsRepository.Settings.Language == 25);
    }

    protected override Task<bool> ApplyInternal()
    {
        SettingsRepository.Settings.Language = AppLanguage.Ukrainian;
        return Task.FromResult(true);
    }
}
