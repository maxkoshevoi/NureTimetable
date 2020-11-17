using Microsoft.AppCenter.Analytics;
using NureTimetable.Core.Extensions;
using NureTimetable.Core.Models.Consts;
using NureTimetable.DAL.Helpers;
using NureTimetable.DAL.Models.Consts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Cist = NureTimetable.DAL.Models.Cist;
using Local = NureTimetable.DAL.Models.Local;

namespace NureTimetable.DAL
{
    public static class EventsRepository
    {
        /// <summary>
        /// Returns events for one entity. Null if error occurs 
        /// </summary>
        public static async Task<Local.TimetableInfo> GetEvents(Local.SavedEntity entity, bool tryUpdate = false, DateTime? dateStart = null, DateTime? dateEnd = null)
        {
            Local.TimetableInfo timetable;
            if (tryUpdate)
            {
                if (dateStart is null || dateEnd is null)
                {
                    throw new ArgumentNullException($"{nameof(dateStart)} and {nameof(dateEnd)} must be set");
                }

                timetable = (await GetTimetableFromCist(entity, dateStart.Value, dateEnd.Value)).Timetable;
                if (timetable != null)
                {
                    return timetable;
                }
            }
            timetable = GetTimetableLocal(entity);
            return timetable;
        }

        #region Local
        public static Local.TimetableInfo GetTimetableLocal(Local.SavedEntity entity)
            => GetTimetableLocal(new List<Local.SavedEntity>() { entity }).FirstOrDefault();

        public static List<Local.TimetableInfo> GetTimetableLocal(List<Local.SavedEntity> entities)
        {
            var timetables = new List<Local.TimetableInfo>();
            if (entities is null)
            {
                return timetables;
            }
            foreach (Local.SavedEntity entity in entities)
            {
                Local.TimetableInfo timetableInfo = Serialisation.FromJsonFile<Local.TimetableInfo>(FilePath.SavedTimetable(entity.Type, entity.ID));
                if (timetableInfo is null)
                {
                    continue;
                }
                timetables.Add(timetableInfo);
            }
            return timetables;
        }

        private static void UpdateTimetableLocal(Local.TimetableInfo newTimetable)
        {
            Serialisation.ToJsonFile(newTimetable, FilePath.SavedTimetable(newTimetable.Entity.Type, newTimetable.Entity.ID));
        }

        #region Lesson Info
        public static void UpdateLessonsInfo(Local.SavedEntity entity, List<Local.LessonInfo> lessonsInfo)
        {
            Local.TimetableInfo timetable = GetTimetableLocal(entity);
            timetable.LessonsInfo = lessonsInfo;
            UpdateTimetableLocal(timetable);
            MessagingCenter.Send(Application.Current, MessageTypes.LessonSettingsChanged, entity);
        }
        #endregion
        #endregion

        #region Cist
        public static async Task<(Local.TimetableInfo Timetable, Exception Exception)> GetTimetableFromCist(Local.SavedEntity entity, DateTime dateStart, DateTime dateEnd)
        {
            if (!SettingsRepository.CheckCistTimetableUpdateRights(new List<Local.SavedEntity> { entity }).Any())
            {
                return (null, null);
            }

            using var client = new HttpClient();
            try
            {
                Local.TimetableInfo timetable = GetTimetableLocal(entity) ?? new Local.TimetableInfo(entity);

                // Getting events
                Analytics.TrackEvent("Cist request", new Dictionary<string, string>
                {
                    { "Type", "GetTimetable" },
                    { "Subtype", entity.Type.ToString() },
                    { "Hour of the day", DateTime.Now.Hour.ToString() }
                });

                Uri uri = Urls.CistApiEntityTimetable(entity.Type, entity.ID, dateStart, dateEnd);
                string responseStr = await client.GetStringOrWebExceptionAsync(uri);
                responseStr = responseStr.Replace("&amp;", "&");
                responseStr = responseStr.Replace("\"events\":[\n]}]", "\"events\": []");
                Cist.Timetable cistTimetable = CistHelper.FromJson<Cist.Timetable>(responseStr);

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
                        Local.Event localEvent = MapConfig.Map<Cist.Event, Local.Event>(ev);
                        localEvent.Lesson = MapConfig.Map<Cist.Lesson, Local.Lesson>(cistTimetable.Lessons.First(l => l.Id == ev.LessonId));
                        localEvent.Type = MapConfig.Map<Cist.EventType, Local.EventType>(
                            cistTimetable.EventTypes.FirstOrDefault(et => et.Id == ev.TypeId) ?? Cist.EventType.UnknownType
                        );
                        localEvent.Teachers = cistTimetable.Teachers
                            .Where(t => ev.TeacherIds.Contains(t.Id))
                            .DistinctBy(t => t.ShortName.Replace('ї', 'i'))
                            .Select(t => MapConfig.Map<Cist.Teacher, Local.Teacher>(t))
                            .ToList();
                        localEvent.Groups = cistTimetable.Groups
                            .Where(g => ev.GroupIds.Contains(g.Id))
                            .Select(g => MapConfig.Map<Cist.Group, Local.Group>(g))
                            .ToList();
                        return localEvent;
                    })
                    .Distinct()
                    .ToList();

                // Saving timetables
                UpdateTimetableLocal(timetable);

                // Updating LastUpdated for saved groups 
                List<Local.SavedEntity> AllSavedEntities = UniversityEntitiesRepository.GetSaved();
                foreach (Local.SavedEntity savedEntity in AllSavedEntities.Where(e => e == entity))
                {
                    savedEntity.LastUpdated = DateTime.Now;
                }
                UniversityEntitiesRepository.UpdateSaved(AllSavedEntities);
                MessagingCenter.Send(Application.Current, MessageTypes.TimetableUpdated, entity);

                return (timetable, null);
            }
            catch (Exception ex)
            {
                ex.Data.Add("Entity", $"{entity.Type} {entity.Name} ({entity.ID})");
                ex.Data.Add("From", dateStart.ToString("dd.MM.yyyy"));
                ex.Data.Add("To", dateEnd.ToString("dd.MM.yyyy"));
                MessagingCenter.Send(Application.Current, MessageTypes.ExceptionOccurred, ex);

                return (null, ex);
            }
        }
        #endregion
    }
}