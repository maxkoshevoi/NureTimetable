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
        public static async Task<IReadOnlyList<Entity>> CheckCistTimetableUpdateRightsAsync(params Entity[] entitiesToUpdate)
        {
            List<Entity> allowedEntities = new();
            entitiesToUpdate ??= Array.Empty<Entity>();

            List<SavedEntity> savedEntities = await UniversityEntitiesRepository.GetSavedAsync();
            foreach (var entity in entitiesToUpdate)
            {
                SavedEntity savedEntity = savedEntities.SingleOrDefault(e => e == entity);
                if (savedEntity == null)
                {
                    // Cannot update timetable for entity that is not saved
                    continue;
                }

                DateTime nowInUkraine = TimeZoneInfo.ConvertTime(DateTime.Now, Config.UkraineTimezone);
                if (savedEntity.LastUpdated == null || 
                    (nowInUkraine.TimeOfDay >= Config.CistDailyTimetableUpdateStartTime && nowInUkraine.TimeOfDay < Config.CistDailyTimetableUpdateEndTime))
                {
                    // Update allowed if never updated before
                    // Unlimited updates between 5 and 6 AM
                    allowedEntities.Add(savedEntity);
                    continue;
                }

                // Update is allowed once per day (day begins at 6 AM)
                DateTime lastUpdatedInUkraine = TimeZoneInfo.ConvertTime(savedEntity.LastUpdated.Value, Config.UkraineTimezone);
                TimeSpan timeBeforeAnotherUpdate;
                if (lastUpdatedInUkraine.TimeOfDay < Config.CistDailyTimetableUpdateEndTime)
                {
                    timeBeforeAnotherUpdate = Config.CistDailyTimetableUpdateEndTime - lastUpdatedInUkraine.TimeOfDay;
                }
                else
                {
                    timeBeforeAnotherUpdate = TimeSpan.FromDays(1) - lastUpdatedInUkraine.TimeOfDay + Config.CistDailyTimetableUpdateEndTime;
                }
                if (nowInUkraine - lastUpdatedInUkraine > timeBeforeAnotherUpdate)
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
