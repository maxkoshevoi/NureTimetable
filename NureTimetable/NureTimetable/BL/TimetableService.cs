using Microsoft.AppCenter.Analytics;
using NureTimetable.Core.Localization;
using NureTimetable.Core.Models.Consts;
using NureTimetable.Core.Models.Exceptions;
using NureTimetable.DAL;
using NureTimetable.DAL.Models.Local;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace NureTimetable.BL
{
    public static class TimetableService
    {
        public static async Task<string> Update(List<Entity> entities)
        {
            if (entities is null || !entities.Any())
            {
                return null;
            }

            List<Entity> entitiesAllowed = SettingsRepository.CheckCistTimetableUpdateRights(entities);
            if (entitiesAllowed.Count == 0)
            {
                return LN.TimetableLatest;
            }

            Analytics.TrackEvent("Updating timetable", new Dictionary<string, string>
            {
                { "Count", entitiesAllowed.Count.ToString() },
                { "Hour of the day", DateTime.Now.Hour.ToString() }
            });

            // Update timetables in background
            const int batchSize = 10;
            List<Task<(TimetableInfo _, Exception Error)>> updateTasks = new();
            for (int i = 0; i < entitiesAllowed.Count; i += batchSize)
            {
                foreach (Entity entity in entitiesAllowed.Skip(i).Take(batchSize))
                {
                    updateTasks.Add(EventsRepository.GetTimetableFromCist(entity, Config.TimetableFromDate, Config.TimetableToDate));
                }
                await Task.WhenAll(updateTasks);
            }

            List<string> success = new(), fail = new();
            bool isNetworkError = false;
            bool isCistError = false;
            for (int i = 0; i < updateTasks.Count; i++)
            {
                Exception ex = updateTasks[i].Result.Error;
                Entity entity = entitiesAllowed[i];
                if (ex is null)
                {
                    success.Add(entity.Name);
                    continue;
                }

                if (ex is WebException)
                {
                    isNetworkError = true;
                }
                else if (ex is CistException)
                {
                    isCistError = true;
                }

                string errorMessage = ex.Message;
                if (errorMessage.Length > 30)
                {
                    errorMessage = errorMessage.Remove(30);
                }
                fail.Add($"{entity.Name} ({errorMessage.Trim()})");
            }

            if (success.Count == entitiesAllowed.Count)
            {
                return null;
            }

            string result = string.Empty;
            if (isNetworkError && fail.Count == entitiesAllowed.Count)
            {
                result = LN.CannotGetDataFromCist;
            }
            else if (isCistError)
            {
                result = LN.CistException;
            }
            else
            {
                if (success.Count > 0)
                {
                    result += string.Format(LN.TimetableUpdated, string.Join(", ", success) + Environment.NewLine);
                }
                if (fail.Count > 0)
                {
                    result += string.Format(LN.ErrorOccurred, string.Join(", ", fail));
                }
            }

            return result;
        }
    }
}
