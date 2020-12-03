using NureTimetable.DAL;
using NureTimetable.DAL.Helpers;
using NureTimetable.DAL.Models.Consts;
using NureTimetable.DAL.Models.Local;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NureTimetable.Migrations
{
    class MoveIsSelectInsideSavedEntityMigration : BaseMigration
    {
        private readonly string SelectedEntitiesPath = Path.Combine(FilePath.LocalStorage, "entities_selected.json");

        public override bool IsNeedsToBeApplied()
        {
            return HandleException(() =>
            {
                return File.Exists(SelectedEntitiesPath);
            });

        }

        public override bool Apply()
        {
            return HandleException(() =>
            {
                var selectedEntities = Serialisation.FromJsonFile<List<SavedEntity>>(SelectedEntitiesPath);
                File.Delete(SelectedEntitiesPath);

                List<SavedEntity> savedEntities = UniversityEntitiesRepository.GetSaved();
                foreach (SavedEntity entity in selectedEntities)
                {
                    SavedEntity savedEntity = savedEntities.FirstOrDefault(e => e == entity);
                    if (savedEntities is not null)
                    {
                        savedEntity.IsSelected = true;
                    }
                }
                UniversityEntitiesRepository.UpdateSaved(savedEntities);
                return true;
            });
        }
    }
}
