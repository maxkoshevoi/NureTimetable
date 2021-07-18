using Microsoft.AppCenter.Analytics;
using Microsoft.Maui.Controls;
using NureTimetable.Core.BL;
using NureTimetable.Core.Extensions;
using NureTimetable.Core.Models.Consts;
using NureTimetable.DAL.Helpers;
using NureTimetable.DAL.Models.Consts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Cist = NureTimetable.DAL.Models.Cist;
using Local = NureTimetable.DAL.Models.Local;

namespace NureTimetable.DAL
{
    public static class EventsRepository
    {
        #region Local
        public static async Task<Local::TimetableInfo?> GetTimetableLocalAsync(Local::Entity entity) => 
            (await GetTimetableLocal(new List<Local::Entity>() { entity })).SingleOrDefault();

        public static async Task<List<Local::TimetableInfo>> GetTimetableLocal(List<Local::Entity> entities)
        {
            List<Local::TimetableInfo> timetables = new();
            if (entities == null)
            {
                return timetables;
            }
            foreach (var entity in entities)
            {
                Local::TimetableInfo? timetableInfo = await Serialisation.FromJsonFile<Local::TimetableInfo>(FilePath.SavedTimetable(entity.Type, entity.ID));
                if (timetableInfo == null)
                {
                    continue;
                }
                timetables.Add(timetableInfo);
            }
            return timetables;
        }

        private static Task UpdateTimetableLocalAsync(Local::TimetableInfo newTimetable)
        {
            return Serialisation.ToJsonFile(newTimetable, FilePath.SavedTimetable(newTimetable.Entity.Type, newTimetable.Entity.ID));
        }

        #region Lesson Info
        public static async Task UpdateLessonsInfo(Local::Entity entity, List<Local::LessonInfo> lessonsInfo)
        {
            Local::TimetableInfo? timetable = await GetTimetableLocalAsync(entity);
            if (timetable == null)
            {
                return;
            }

            timetable.LessonsInfo = lessonsInfo;
            await UpdateTimetableLocalAsync(timetable);
            MessagingCenter.Send(Application.Current, MessageTypes.LessonSettingsChanged, entity);
        }
        #endregion
        #endregion

        #region Cist
        public static async Task<(Local::TimetableInfo? timetable, Exception? exception)> GetTimetableFromCistAsync(Local::Entity entity, DateTime dateStart, DateTime dateEnd)
        {
            if ((await SettingsRepository.CheckCistTimetableUpdateRightsAsync(entity)).Count == 0)
            {
                return (null, null);
            }

            try
            {
                MessagingCenter.Send(Application.Current, MessageTypes.TimetableUpdating, entity);
                Analytics.TrackEvent("Cist request", new Dictionary<string, string>
                {
                    { "Type", "GetTimetable" },
                    { "Subtype", entity.Type.ToString() },
                    { "Hour of the day", DateTime.Now.Hour.ToString() }
                });

                // Getting events
                Local::TimetableInfo timetable = await GetTimetableLocalAsync(entity) ?? new(entity);

                using HttpClient client = new();
                Uri uri = Urls.CistApiEntityTimetable(entity.Type, entity.ID, dateStart, dateEnd);
                string responseStr = await client.GetStringOrWebExceptionAsync(uri);
                responseStr = responseStr.Replace("&amp;", "&");
                responseStr = responseStr.Replace("\"events\":[\n]}]", "\"events\": []");
                Cist::Timetable cistTimetable = CistHelper.FromJson<Cist::Timetable>(responseStr);

                // Check for valid results
                if (timetable.Events.Count != 0 && cistTimetable.Events.Count == 0)
                {
                    Analytics.TrackEvent("Received timetable is empty", new Dictionary<string, string>
                    {
                        { "Entity", $"{entity.Type} {entity.Name} ({entity.ID})" },
                        { "From", dateStart.ToString("dd.MM.yyyy") },
                        { "To", dateEnd.ToString("dd.MM.yyyy") }
                    });

                    return (null, null);
                }

                // Updating timetable information
                timetable.Events = cistTimetable.Events.Select(ev =>
                    {
                        Local::Event localEvent = MapConfig.Map<Cist::Event, Local::Event>(ev);
                        localEvent.Lesson = MapConfig.Map<Cist::Lesson, Local::Lesson>(cistTimetable.Lessons.First(l => l.Id == ev.LessonId));
                        var cistType = cistTimetable.EventTypes.FirstOrDefault(et => et.Id == ev.TypeId);
                        if (cistType != null)
                        {
                            localEvent.Type = MapConfig.Map<Cist::EventType, Local::EventType>(cistType);
                        }
                        localEvent.Teachers = cistTimetable.Teachers
                            .Where(t => ev.TeacherIds.Contains(t.Id))
                            .DistinctBy(t => t.ShortName.Replace('ї', 'i'))
                            .Select(t => MapConfig.Map<Cist::Teacher, Local::Teacher>(t))
                            .ToList();
                        localEvent.Groups = cistTimetable.Groups
                            .Where(g => ev.GroupIds.Contains(g.Id))
                            .Select(g => MapConfig.Map<Cist::Group, Local::Group>(g))
                            .ToList();
                        return localEvent;
                    })
                    .Distinct()
                    .ToList();

                // Saving timetables
                await UpdateTimetableLocalAsync(timetable);

                // Updating LastUpdated for saved entity
                await UniversityEntitiesRepository.ModifySavedAsync(savedEntities =>
                {
                    var savedEntity = savedEntities.SingleOrDefault(e => e == entity);
                    if (savedEntity == null)
                    {
                        // Saved entity may be deleted while timetable is updating
                        return true;
                    }

                    savedEntity.LastUpdated = DateTime.Now;
                    return false;
                });

                return (timetable, null);
            }
            catch (Exception ex)
            {
                ex.Data.Add("Entity", $"{entity.Type} {entity.Name} ({entity.ID})");
                ex.Data.Add("From", dateStart.ToString("dd.MM.yyyy"));
                ex.Data.Add("To", dateEnd.ToString("dd.MM.yyyy"));
                ExceptionService.LogException(ex);

                return (null, ex);
            }
            finally
            {
                MessagingCenter.Send(Application.Current, MessageTypes.TimetableUpdated, entity);
            }
        }
        #endregion
    }
}