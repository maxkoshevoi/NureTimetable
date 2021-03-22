using NureTimetable.DAL.Helpers;
using NureTimetable.DAL.Models.Consts;
using NureTimetable.DAL.Models.Local;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NureTimetable.Migrations
{
    class MoveEntityInsideSavedEntityMigration : BaseMigration
    {
        protected override async Task<bool> IsNeedsToBeAppliedInternal()
        {
            var entities = await Serialisation.FromJsonFile<List<Entity>>(FilePath.SavedEntitiesList);
            return entities?.Any() == true && entities.First().ID > 0;
        }

        protected override async Task<bool> ApplyInternal()
        {
            try
            {
                List<SavedEntity> updatedSavedEntities = new();
                var selectedEntities = await Serialisation.FromJsonFile<List<SavedEntityWithoutEntity>>(FilePath.SavedEntitiesList);
                var entities = await Serialisation.FromJsonFile<List<Entity>>(FilePath.SavedEntitiesList);

                for (int i = 0; i < entities.Count; i++)
                {
                    updatedSavedEntities.Add(new(entities[i])
                    {
                        IsSelected = selectedEntities[i].IsSelected,
                        LastUpdated = selectedEntities[i].LastUpdated
                    });
                }

                await Serialisation.ToJsonFile(updatedSavedEntities, FilePath.SavedEntitiesList);
                return true;
            }
            catch (NullReferenceException)
            {
                File.Delete(FilePath.SavedEntitiesList);
                return false;
            }
        }

        class SavedEntityWithoutEntity
        {
            public DateTime? LastUpdated { get; set; }

            public bool IsSelected { get; set; }
        }
    }
}
