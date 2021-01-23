using NureTimetable.DAL;
using NureTimetable.DAL.Helpers;
using NureTimetable.DAL.Models.Consts;
using NureTimetable.DAL.Models.Local;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NureTimetable.Migrations
{
    class MoveEntityInsideSavedEntityMigration : BaseMigration
    {
        protected override bool IsNeedsToBeAppliedInternal()
        {
            var entities = Serialisation.FromJsonFile<List<Entity>>(FilePath.SavedEntitiesList);
            return entities?.Any() == true && entities.First().ID > 0;
        }

        protected override bool ApplyInternal()
        {
            List<SavedEntity> updatedSavedEntities = new();
            var selectedEntities = Serialisation.FromJsonFile<List<SavedEntityWithoutEntity>>(FilePath.SavedEntitiesList);
            var entities = Serialisation.FromJsonFile<List<Entity>>(FilePath.SavedEntitiesList);

            for (int i = 0; i < entities.Count; i++)
            {
                updatedSavedEntities.Add(new(entities[i])
                {
                    IsSelected = selectedEntities[i].IsSelected,
                    LastUpdated = selectedEntities[i].LastUpdated
                });
            }

            Serialisation.ToJsonFile(updatedSavedEntities, FilePath.SavedEntitiesList);
            return true;
        }

        class SavedEntityWithoutEntity
        {
            public DateTime? LastUpdated { get; set; }

            public bool IsSelected { get; set; }
        }
    }
}
