using NureTimetable.DAL;
using NureTimetable.DAL.Helpers;
using NureTimetable.DAL.Models.Consts;
using NureTimetable.DAL.Models.Local;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NureTimetable.Migrations
{
    class MoveEntityInsideSavedEntityMigration : BaseMigration
    {
        protected override bool IsNeedsToBeAppliedInternal()
        {
            return UniversityEntitiesRepository.GetSaved().Any(e => e.Entity is null);
        }

        protected override bool ApplyInternal()
        {
            var selectedEntities = UniversityEntitiesRepository.GetSaved();
            var entities = Serialisation.FromJsonFile<List<Entity>>(FilePath.SavedEntitiesList);

            for (int i = 0; i < entities.Count; i++)
            {
                selectedEntities[i] = new(entities[i])
                {
                    IsSelected = selectedEntities[i].IsSelected,
                    LastUpdated = selectedEntities[i].LastUpdated
                };
            }

            Serialisation.ToJsonFile(selectedEntities, FilePath.SavedEntitiesList);
            return true;
        }
    }
}
