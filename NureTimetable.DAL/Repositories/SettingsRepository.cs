using NureTimetable.Core.Models;
using NureTimetable.Core.Models.Consts;
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
        #region Timetable Update Rights
        public static List<SavedEntity> CheckCistTimetableUpdateRights(List<SavedEntity> entitiesToUpdate)
        {
            var allowedEntities = new List<SavedEntity>();
            if (entitiesToUpdate == null || entitiesToUpdate.Count == 0)
            {
                return allowedEntities;
            }

            List<SavedEntity> savedEntities = UniversityEntitiesRepository.GetSaved();
            foreach (SavedEntity entity in entitiesToUpdate)
            {
                SavedEntity savedEntity = savedEntities.FirstOrDefault(g => g.ID == entity.ID);
                if (savedEntity == null)
                {
                    // Cannot update timetable for entity that is not saved
                    continue;
                }

                if (savedEntity.LastUpdated == null || (DateTime.Now.TimeOfDay.Hours >= 5 && DateTime.Now.TimeOfDay.Hours < 7))
                {
                    // Update allowed if never updated before
                    // Unlimited updates between 5 and 7 AM
                    allowedEntities.Add(savedEntity);
                    continue;
                }

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
            TimeSpan? timePass = DateTime.Now - GetLastCistAllEntitiesUpdateTime();
#pragma warning restore CS0162 // Unreachable code detected
            if (timePass != null && timePass <= Config.CistAllEntitiesUpdateMinInterval)
            {
                return false;
            }
            return true;
        }

        private static DateTime? GetLastCistAllEntitiesUpdateTime()
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

        #region Application Settings
        public static AppSettings GetSettings()
        {
            return Serialisation.FromJsonFile<AppSettings>(FilePath.AppSettings) ?? new AppSettings();
        }

        public static void UpdateSettings(AppSettings settings)
        {
            Serialisation.ToJsonFile(settings, FilePath.AppSettings);
        }
        #endregion
    }
}
