using NureTimetable.DAL;
using NureTimetable.DAL.Helpers;
using NureTimetable.DAL.Models.Consts;
using NureTimetable.DAL.Models.Local;
using System.IO;

namespace NureTimetable.MIgrations
{
    class CanSelectMultipleEntitiesMigration : BaseMigration
    {
        public override bool IsNeedsToBeApplied()
        {
            return HandleException(() =>
            {
                return !File.Exists(FilePath.SelectedEntities);
            });

        }

        public override bool Apply()
        {
            return HandleException(() =>
            {
                SavedEntity savedEntity = Serialisation.FromJsonFile<SavedEntity>(Path.Combine(FilePath.LocalStorage, "entity_selected.json"));
                UniversityEntitiesRepository.UpdateSelected(savedEntity);
                return true;
            });
        }
    }
}
