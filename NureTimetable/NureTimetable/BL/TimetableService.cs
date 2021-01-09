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
using Xamarin.Forms;

namespace NureTimetable.BL
{
    public static class TimetableService
    {
        public static async Task UpdateAndDisplayResult(params Entity[] entities)
        {
            var updateResult = await Update(entities);
            string response = GetResponseMessageFromUpdateResult(updateResult);
            
            if (response is not null)
            {
                await Shell.Current.DisplayAlert(LN.TimetableUpdate, response, LN.Ok);
            }
        }

        public static async Task<List<(Entity entity, Exception exception)>> Update(params Entity[] entities)
        {
            IReadOnlyList<Entity> entitiesAllowed = SettingsRepository.CheckCistTimetableUpdateRights(entities);
            if (entitiesAllowed.Count == 0)
            {
                return new();
            }

            Analytics.TrackEvent("Updating timetable", new Dictionary<string, string>
            {
                { "Count", entitiesAllowed.Count.ToString() },
                { "Hour of the day", DateTime.Now.Hour.ToString() }
            });

            // Update timetables in background
            const int batchSize = 10;
            Dictionary<Entity, Task<(TimetableInfo _, Exception Error)>> updateTasks = new();
            for (int i = 0; i < entitiesAllowed.Count; i += batchSize)
            {
                foreach (Entity entity in entitiesAllowed.Skip(i).Take(batchSize))
                {
                    updateTasks.Add(entity, EventsRepository.GetTimetableFromCist(entity, Config.TimetableFromDate, Config.TimetableToDate));
                }
                await Task.WhenAll(updateTasks.Select(u => u.Value));
            }

            List<(Entity, Exception)> updateResults = updateTasks.Select(r => (r.Key, r.Value.Result.Error)).ToList();
            return updateResults;
        }

        private static string GetResponseMessageFromUpdateResult(List<(Entity entity, Exception exception)> updateResults)
        {
            if (updateResults.All(r => r.exception is null))
            {
                return null;
            }

            List<string> success = new(), fail = new();
            bool isNetworkError = false;
            bool isCistError = false;
            foreach (var (entity, ex) in updateResults)
            {
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

            string result = string.Empty;
            if (isNetworkError && fail.Count == updateResults.Count)
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
                    result += string.Format(LN.TimetableUpdated, $"{string.Join(", ", success)}\n\n");
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
