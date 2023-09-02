using NureTimetable.DAL;
using NureTimetable.DAL.Consts;
using NureTimetable.DAL.Models;

namespace NureTimetable.Migrations;

class MoveEntityInsideSavedEntityMigration : BaseMigration
{
    protected override async Task<bool> IsNeedsToBeAppliedInternal()
    {
        var entities = await Serialization.FromJsonFile<List<Entity>>(FilePath.SavedEntitiesList);
        return entities?.Any() == true && entities.First().ID > 0;
    }

    protected override async Task<bool> ApplyInternal()
    {
        List<SavedEntity> updatedSavedEntities = new();
        var selectedEntities = await Serialization.FromJsonFile<List<SavedEntityWithoutEntity>>(FilePath.SavedEntitiesList);
        var entities = await Serialization.FromJsonFile<List<Entity>>(FilePath.SavedEntitiesList);
        if (selectedEntities == null || entities == null)
        {
            File.Delete(FilePath.SavedEntitiesList);
            return false;
        }

        for (int i = 0; i < entities.Count; i++)
        {
            updatedSavedEntities.Add(new(entities[i])
            {
                IsSelected = selectedEntities[i].IsSelected,
                LastUpdated = selectedEntities[i].LastUpdated
            });
        }

        await Serialization.ToJsonFile(updatedSavedEntities, FilePath.SavedEntitiesList);
        return true;
    }

    private class SavedEntityWithoutEntity
    {
        public DateTime? LastUpdated { get; set; }

        public bool IsSelected { get; set; }
    }
}
