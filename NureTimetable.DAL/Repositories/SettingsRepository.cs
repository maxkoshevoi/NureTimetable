using NureTimetable.Core.Models.Consts;
using NureTimetable.Core.Models.Settings;
using NureTimetable.DAL.Helpers;
using NureTimetable.DAL.Models.Consts;
using NureTimetable.DAL.Models.Local;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NureTimetable.DAL
{
    public static class SettingsRepository
    {
        public static AppSettings Settings { get; } = new();

        #region Timetable Update Rights
        public static List<Entity> CheckCistTimetableUpdateRights(List<Entity> entitiesToUpdate)
        {
            List<Entity> allowedEntities = new();
            if (entitiesToUpdate is null || entitiesToUpdate.Count == 0)
            {
                return allowedEntities;
            }

            List<SavedEntity> savedEntities = UniversityEntitiesRepository.GetSaved();
            foreach (Entity entity in entitiesToUpdate)
            {
                SavedEntity savedEntity = savedEntities.SingleOrDefault(e => e == entity);
                if (savedEntity is null)
                {
                    // Cannot update timetable for entity that is not saved
                    continue;
                }

                if (savedEntity.LastUpdated is null || (DateTime.Now.TimeOfDay.Hours >= 5 && DateTime.Now.TimeOfDay.Hours < 7))
                {
                    // Update allowed if never updated before
                    // Unlimited updates between 5 and 7 AM
                    allowedEntities.Add(savedEntity);
                    continue;
                }

                // Update is allowd once per day (day begins at 7 AM)
                TimeSpan timeBeforeAnotherUpdate;
                if (savedEntity.LastUpdated.Value.TimeOfDay < Config.CistDailyTimetableUpdateTime)
                {
                    timeBeforeAnotherUpdate = Config.CistDailyTimetableUpdateTime - savedEntity.LastUpdated.Value.TimeOfDay;
                }
                else
                {
                    timeBeforeAnotherUpdate = TimeSpan.FromDays(1) - savedEntity.LastUpdated.Value.TimeOfDay + Config.CistDailyTimetableUpdateTime;
                }
                if (DateTime.Now - savedEntity.LastUpdated.Value > timeBeforeAnotherUpdate)
                {
                    allowedEntities.Add(savedEntity);
                    continue;
                }
#if DEBUG
                allowedEntities.Add(savedEntity);
#endif
            }
            return allowedEntities;
        }
        #endregion

        #region All Entities Update Rights
        public static bool CheckCistAllEntitiesUpdateRights()
        {
#if DEBUG
            return true;
#endif

#pragma warning disable CS0162 // Unreachable code detected
            if (DateTime.Now.Month == 8 || DateTime.Now.Month == 9)
            {
                // Unlimited update in August and September
                return true;
            }

            TimeSpan? timePass = DateTime.Now - GetLastCistAllEntitiesUpdateTime();
            if (timePass != null && timePass <= Config.CistAllEntitiesUpdateMinInterval)
            {
                return false;
            }
            return true;
#pragma warning restore CS0162 // Unreachable code detected
        }

        public static DateTime? GetLastCistAllEntitiesUpdateTime()
        {
            string filePath = FilePath.LastCistAllEntitiesUpdate;
            if (!File.Exists(filePath))
            {
                return null;
            }

            DateTime lastTimetableUpdate = Serialisation.FromJsonFile<DateTime>(filePath);
            return lastTimetableUpdate;
        }

        public static void UpdateCistAllEntitiesUpdateTime()
        {
            Serialisation.ToJsonFile(DateTime.Now, FilePath.LastCistAllEntitiesUpdate);
        }
        #endregion
    }
}
