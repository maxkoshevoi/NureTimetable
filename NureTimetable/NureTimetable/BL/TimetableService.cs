using Microsoft.AppCenter.Analytics;
using NureTimetable.Core.Localization;
using NureTimetable.Core.Models.Consts;
using NureTimetable.Core.Models.Exceptions;
using NureTimetable.DAL.Cist;
using NureTimetable.DAL.Models;
using NureTimetable.DAL.Settings;
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
        public static async Task UpdateAndDisplayResultAsync(params Entity[] entities)
        {
            var updateResult = await UpdateAsync(entities);
            string? response = GetResponseMessageFromUpdateResult(updateResult);
            
            if (response != null)
            {
                await Shell.Current.DisplayAlert(LN.TimetableUpdate, response, LN.Ok);
            }
        }

        public static Task<List<(Entity entity, Exception? exception)>> UpdateAsync(params Entity[] entities) => Task.Run(async () =>
        {
            IReadOnlyList<Entity> entitiesAllowed = await SettingsRepository.CheckCistTimetableUpdateRightsAsync(entities);
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
            const int batchSize = 5;
            Dictionary<Entity, Task<(TimetableInfo? _, Exception? error)>> updateTasks = new();
            for (int i = 0; i < entitiesAllowed.Count;)
            {
                int runningTasks = updateTasks.Count(t => !t.Value.IsCompleted);
                int capacity = batchSize - runningTasks;
                foreach (var entity in entitiesAllowed.Skip(i).Take(capacity))
                {
                    updateTasks.Add(entity, EventsRepository.GetTimetableFromCistAsync(entity, Config.TimetableFromDate, Config.TimetableToDate));
                }
                await Task.WhenAny(updateTasks
                    .Select(u => u.Value)
                    .Where(t => !t.IsCompleted)
                    .DefaultIfEmpty(Task.CompletedTask));

                if (updateTasks.Any(u => u.Value.IsCompleted && u.Value.Result.error is WebException))
                {
                    // Abort updating on network error
                    break;
                }

                i += capacity;
            }
            await Task.WhenAll(updateTasks.Select(u => u.Value));

            List<(Entity, Exception?)> updateResults = updateTasks.Select(r => (r.Key, r.Value.Result.error)).ToList();
            return updateResults;
        });

        private static string? GetResponseMessageFromUpdateResult(List<(Entity entity, Exception? exception)> updateResults)
        {
            if (updateResults.Count == 0)
            {
                return LN.TimetableLatest;
            }
            if (updateResults.All(r => r.exception == null))
            {
                return null;
            }

            List<string> success = new(), fail = new();
            bool isNetworkError = false;
            bool isCistError = false;
            foreach (var (entity, ex) in updateResults)
            {
                if (ex == null)
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
