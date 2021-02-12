using NureTimetable.Core.Models.Consts;
using NureTimetable.Core.Models.Settings;
using NureTimetable.DAL.Models.Local;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NureTimetable.DAL
{
    public static class SettingsRepository
    {
        public static AppSettings Settings { get; } = AppSettings.Instance;

        #region Timetable Update Rights
        public static async Task<IReadOnlyList<Entity>> CheckCistTimetableUpdateRights(params Entity[] entitiesToUpdate)
        {
            List<Entity> allowedEntities = new();
            entitiesToUpdate ??= Array.Empty<Entity>();

            List<SavedEntity> savedEntities = await UniversityEntitiesRepository.GetSaved();
            foreach (var entity in entitiesToUpdate)
            {
                SavedEntity savedEntity = savedEntities.SingleOrDefault(e => e == entity);
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
            if (DateTime.Now.Month == 8 || DateTime.Now.Month == 9)
            {
                // Unlimited update in August and September
                return true;
            }

            TimeSpan? timePass = DateTime.Now - Settings.LastCistAllEntitiesUpdate;
            if (timePass == null || timePass > Config.CistAllEntitiesUpdateMinInterval)
            {
                return true;
            }

#if DEBUG
            return true;
#else
            return false;
#endif
        }
#endregion
    }
}
