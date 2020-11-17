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
                string responseStr = GetHardcodedEventsFromCist();
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

        private static string GetHardcodedEventsFromCist()
        {
            return "{\n\"time-zone\":\"Europe/Kiev\",\n\"events\":[\n{\"subject_id\":8051836,\"start_time\":1601367300,\"end_time\":1601373000,\"type\":10,\"number_pair\":3,\"auditory\":\"спорт2\",\"teachers\":[],\"groups\":[8635574\n]}\n,\n{\"subject_id\":4809196,\"start_time\":1601380500,\"end_time\":1601386200,\"type\":0,\"number_pair\":5,\"auditory\":\"511і\",\"teachers\":[2908],\"groups\":[8635576\n,8511166\n,8635572\n,8635574\n]}\n,\n{\"subject_id\":4809196,\"start_time\":1601453700,\"end_time\":1601459400,\"type\":0,\"number_pair\":3,\"auditory\":\"365\",\"teachers\":[2908],\"groups\":[8635576\n,8511166\n,8635572\n,8635574\n]}\n,\n{\"subject_id\":5306218,\"start_time\":1601460600,\"end_time\":1601466300,\"type\":0,\"number_pair\":4,\"auditory\":\"365\",\"teachers\":[5306219],\"groups\":[8635576\n,8635572\n,8635574\n,8511166\n]}\n,\n{\"subject_id\":6963436,\"start_time\":1601466900,\"end_time\":1601472600,\"type\":0,\"number_pair\":5,\"auditory\":\"365\",\"teachers\":[84],\"groups\":[8635576\n,8635572\n,8511166\n,8635574\n]}\n,\n{\"subject_id\":4809154,\"start_time\":1601527500,\"end_time\":1601533200,\"type\":0,\"number_pair\":1,\"auditory\":\"104і\",\"teachers\":[172],\"groups\":[8635574\n,8511166\n,8635572\n,8635576\n]}\n,\n{\"subject_id\":5304362,\"start_time\":1601533800,\"end_time\":1601539500,\"type\":0,\"number_pair\":2,\"auditory\":\"365\",\"teachers\":[372],\"groups\":[8635572\n,8635574\n,8511166\n,8635576\n]}\n,\n{\"subject_id\":4809160,\"start_time\":1601540100,\"end_time\":1601545800,\"type\":0,\"number_pair\":3,\"auditory\":\"365\",\"teachers\":[326],\"groups\":[8511166\n,8635572\n,8635576\n,8635574\n]}\n,\n{\"subject_id\":5318974,\"start_time\":1601547000,\"end_time\":1601552700,\"type\":0,\"number_pair\":4,\"auditory\":\"365\",\"teachers\":[106],\"groups\":[8511166\n,8635572\n,8635576\n,8635574\n]}\n,\n{\"subject_id\":8051836,\"start_time\":1601972100,\"end_time\":1601977800,\"type\":10,\"number_pair\":3,\"auditory\":\"спорт2\",\"teachers\":[],\"groups\":[8635574\n]}\n,\n{\"subject_id\":4809196,\"start_time\":1601979000,\"end_time\":1601984700,\"type\":0,\"number_pair\":4,\"auditory\":\"511і\",\"teachers\":[2908],\"groups\":[8635576\n,8511166\n,8635574\n,8635572\n]}\n,\n{\"subject_id\":4809196,\"start_time\":1601985300,\"end_time\":1601991000,\"type\":0,\"number_pair\":5,\"auditory\":\"511і\",\"teachers\":[2908],\"groups\":[8511166\n,8635576\n,8635572\n,8635574\n]}\n,\n{\"subject_id\":4809196,\"start_time\":1602058500,\"end_time\":1602064200,\"type\":0,\"number_pair\":3,\"auditory\":\"365\",\"teachers\":[2908],\"groups\":[8635576\n,8511166\n,8635572\n,8635574\n]}\n,\n{\"subject_id\":5306218,\"start_time\":1602065400,\"end_time\":1602071100,\"type\":0,\"number_pair\":4,\"auditory\":\"365\",\"teachers\":[5306219],\"groups\":[8635576\n,8635572\n,8635574\n,8511166\n]}\n,\n{\"subject_id\":6963436,\"start_time\":1602071700,\"end_time\":1602077400,\"type\":0,\"number_pair\":5,\"auditory\":\"365\",\"teachers\":[84],\"groups\":[8635576\n,8635572\n,8511166\n,8635574\n]}\n,\n{\"subject_id\":4809154,\"start_time\":1602132300,\"end_time\":1602138000,\"type\":0,\"number_pair\":1,\"auditory\":\"104і\",\"teachers\":[172],\"groups\":[8635574\n,8511166\n,8635572\n,8635576\n]}\n,\n{\"subject_id\":5304362,\"start_time\":1602138600,\"end_time\":1602144300,\"type\":0,\"number_pair\":2,\"auditory\":\"365\",\"teachers\":[372],\"groups\":[8635572\n,8635574\n,8511166\n,8635576\n]}\n,\n{\"subject_id\":4809160,\"start_time\":1602144900,\"end_time\":1602150600,\"type\":0,\"number_pair\":3,\"auditory\":\"365\",\"teachers\":[326],\"groups\":[8511166\n,8635572\n,8635576\n,8635574\n]}\n,\n{\"subject_id\":5318974,\"start_time\":1602151800,\"end_time\":1602157500,\"type\":0,\"number_pair\":4,\"auditory\":\"365\",\"teachers\":[106],\"groups\":[8511166\n,8635572\n,8635576\n,8635574\n]}\n,\n{\"subject_id\":8051836,\"start_time\":1602576900,\"end_time\":1602582600,\"type\":10,\"number_pair\":3,\"auditory\":\"спорт2\",\"teachers\":[],\"groups\":[8635574\n]}\n,\n{\"subject_id\":4809154,\"start_time\":1602737100,\"end_time\":1602742800,\"type\":0,\"number_pair\":1,\"auditory\":\"106і\",\"teachers\":[172],\"groups\":[8635574\n,8511166\n,8635572\n,8635576\n]}\n,\n{\"subject_id\":5304362,\"start_time\":1602743400,\"end_time\":1602749100,\"type\":0,\"number_pair\":2,\"auditory\":\"365\",\"teachers\":[372],\"groups\":[8635572\n,8635574\n,8511166\n,8635576\n]}\n,\n{\"subject_id\":4809160,\"start_time\":1602749700,\"end_time\":1602755400,\"type\":0,\"number_pair\":3,\"auditory\":\"365\",\"teachers\":[326],\"groups\":[8511166\n,8635572\n,8635576\n,8635574\n]}\n,\n{\"subject_id\":5318974,\"start_time\":1602756600,\"end_time\":1602762300,\"type\":0,\"number_pair\":4,\"auditory\":\"365\",\"teachers\":[106],\"groups\":[8511166\n,8635572\n,8635576\n,8635574\n]}\n,\n{\"subject_id\":8051836,\"start_time\":1603181700,\"end_time\":1603187400,\"type\":10,\"number_pair\":3,\"auditory\":\"спорт2\",\"teachers\":[],\"groups\":[8635574\n]}\n,\n{\"subject_id\":4809196,\"start_time\":1603268100,\"end_time\":1603273800,\"type\":0,\"number_pair\":3,\"auditory\":\"339\",\"teachers\":[2908],\"groups\":[8635576\n,8511166\n,8635572\n,8635574\n]}\n,\n{\"subject_id\":5306218,\"start_time\":1603275000,\"end_time\":1603280700,\"type\":0,\"number_pair\":4,\"auditory\":\"365\",\"teachers\":[5306219],\"groups\":[8635576\n,8635572\n,8635574\n,8511166\n]}\n,\n{\"subject_id\":6963436,\"start_time\":1603281300,\"end_time\":1603287000,\"type\":0,\"number_pair\":5,\"auditory\":\"365\",\"teachers\":[84],\"groups\":[8635576\n,8635572\n,8511166\n,8635574\n]}\n,\n{\"subject_id\":4809154,\"start_time\":1603341900,\"end_time\":1603347600,\"type\":0,\"number_pair\":1,\"auditory\":\"141\",\"teachers\":[172],\"groups\":[8635572\n,8635574\n,8635576\n,8511166\n]}\n,\n{\"subject_id\":5304362,\"start_time\":1603348200,\"end_time\":1603353900,\"type\":0,\"number_pair\":2,\"auditory\":\"365\",\"teachers\":[372],\"groups\":[8635576\n,8635572\n,8635574\n,8511166\n]}\n,\n{\"subject_id\":4809160,\"start_time\":1603354500,\"end_time\":1603360200,\"type\":0,\"number_pair\":3,\"auditory\":\"365\",\"teachers\":[326],\"groups\":[8635572\n,8635576\n,8635574\n,8511166\n]}\n,\n{\"subject_id\":8051836,\"start_time\":1603790100,\"end_time\":1603795800,\"type\":10,\"number_pair\":3,\"auditory\":\"спорт2\",\"teachers\":[],\"groups\":[8635574\n]}\n,\n{\"subject_id\":4809196,\"start_time\":1603876500,\"end_time\":1603882200,\"type\":0,\"number_pair\":3,\"auditory\":\"339\",\"teachers\":[2908],\"groups\":[8635576\n,8511166\n,8635572\n,8635574\n]}\n,\n{\"subject_id\":5306218,\"start_time\":1603883400,\"end_time\":1603889100,\"type\":0,\"number_pair\":4,\"auditory\":\"365\",\"teachers\":[5306219],\"groups\":[8635576\n,8635572\n,8635574\n,8511166\n]}\n,\n{\"subject_id\":6963436,\"start_time\":1603889700,\"end_time\":1603895400,\"type\":0,\"number_pair\":5,\"auditory\":\"365\",\"teachers\":[84],\"groups\":[8635576\n,8635572\n,8511166\n,8635574\n]}\n,\n{\"subject_id\":4809154,\"start_time\":1603950300,\"end_time\":1603956000,\"type\":0,\"number_pair\":1,\"auditory\":\"140\",\"teachers\":[172],\"groups\":[8635572\n,8635574\n,8635576\n,8511166\n]}\n,\n{\"subject_id\":5304362,\"start_time\":1603956600,\"end_time\":1603962300,\"type\":0,\"number_pair\":2,\"auditory\":\"365\",\"teachers\":[372],\"groups\":[8635576\n,8635572\n,8635574\n,8511166\n]}\n,\n{\"subject_id\":4809160,\"start_time\":1603962900,\"end_time\":1603968600,\"type\":0,\"number_pair\":3,\"auditory\":\"365\",\"teachers\":[326],\"groups\":[8635572\n,8635576\n,8635574\n,8511166\n]}\n,\n{\"subject_id\":8051836,\"start_time\":1604394900,\"end_time\":1604400600,\"type\":10,\"number_pair\":3,\"auditory\":\"спорт2\",\"teachers\":[],\"groups\":[8635574\n]}\n,\n{\"subject_id\":4809196,\"start_time\":1604481300,\"end_time\":1604487000,\"type\":0,\"number_pair\":3,\"auditory\":\"339\",\"teachers\":[2908],\"groups\":[8635576\n,8511166\n,8635572\n,8635574\n]}\n,\n{\"subject_id\":5306218,\"start_time\":1604488200,\"end_time\":1604493900,\"type\":0,\"number_pair\":4,\"auditory\":\"365\",\"teachers\":[5306219],\"groups\":[8635576\n,8635572\n,8635574\n,8511166\n]}\n,\n{\"subject_id\":6963436,\"start_time\":1604494500,\"end_time\":1604500200,\"type\":0,\"number_pair\":5,\"auditory\":\"365\",\"teachers\":[84],\"groups\":[8635576\n,8635572\n,8511166\n,8635574\n]}\n,\n{\"subject_id\":4809154,\"start_time\":1604555100,\"end_time\":1604560800,\"type\":0,\"number_pair\":1,\"auditory\":\"140\",\"teachers\":[172],\"groups\":[8635574\n,8511166\n,8635572\n,8635576\n]}\n,\n{\"subject_id\":5304362,\"start_time\":1604561400,\"end_time\":1604567100,\"type\":0,\"number_pair\":2,\"auditory\":\"365\",\"teachers\":[372],\"groups\":[8635572\n,8635574\n,8511166\n,8635576\n]}\n,\n{\"subject_id\":4809160,\"start_time\":1604567700,\"end_time\":1604573400,\"type\":0,\"number_pair\":3,\"auditory\":\"365\",\"teachers\":[326],\"groups\":[8511166\n,8635572\n,8635576\n,8635574\n]}\n,\n{\"subject_id\":5318974,\"start_time\":1604574600,\"end_time\":1604580300,\"type\":0,\"number_pair\":4,\"auditory\":\"365\",\"teachers\":[106],\"groups\":[8511166\n,8635572\n,8635576\n,8635574\n]}\n,\n{\"subject_id\":8051836,\"start_time\":1604999700,\"end_time\":1605005400,\"type\":10,\"number_pair\":3,\"auditory\":\"спорт2\",\"teachers\":[],\"groups\":[8635574\n]}\n,\n{\"subject_id\":5318974,\"start_time\":1605079800,\"end_time\":1605085500,\"type\":0,\"number_pair\":2,\"auditory\":\"103і\",\"teachers\":[106],\"groups\":[8511166\n,8635576\n,8635574\n,8635572\n]}\n,\n{\"subject_id\":4809196,\"start_time\":1605086100,\"end_time\":1605091800,\"type\":0,\"number_pair\":3,\"auditory\":\"339\",\"teachers\":[2908],\"groups\":[8511166\n,8635572\n,8635574\n,8635576\n]}\n,\n{\"subject_id\":5306218,\"start_time\":1605093000,\"end_time\":1605098700,\"type\":0,\"number_pair\":4,\"auditory\":\"365\",\"teachers\":[5306219],\"groups\":[8635576\n,8635572\n,8635574\n,8511166\n]}\n,\n{\"subject_id\":6963436,\"start_time\":1605099300,\"end_time\":1605105000,\"type\":0,\"number_pair\":5,\"auditory\":\"365\",\"teachers\":[84],\"groups\":[8635574\n,8635572\n,8635576\n,8511166\n]}\n,\n{\"subject_id\":4809154,\"start_time\":1605159900,\"end_time\":1605165600,\"type\":0,\"number_pair\":1,\"auditory\":\"141\",\"teachers\":[172],\"groups\":[8635574\n,8511166\n,8635572\n,8635576\n]}\n,\n{\"subject_id\":5304362,\"start_time\":1605166200,\"end_time\":1605171900,\"type\":0,\"number_pair\":2,\"auditory\":\"365\",\"teachers\":[372],\"groups\":[8635572\n,8635574\n,8511166\n,8635576\n]}\n,\n{\"subject_id\":4809160,\"start_time\":1605172500,\"end_time\":1605178200,\"type\":0,\"number_pair\":3,\"auditory\":\"365\",\"teachers\":[326],\"groups\":[8511166\n,8635572\n,8635576\n,8635574\n]}\n,\n{\"subject_id\":5318974,\"start_time\":1605179400,\"end_time\":1605185100,\"type\":0,\"number_pair\":4,\"auditory\":\"365\",\"teachers\":[106],\"groups\":[8511166\n,8635572\n,8635576\n,8635574\n]}\n,\n{\"subject_id\":8051836,\"start_time\":1605604500,\"end_time\":1605610200,\"type\":10,\"number_pair\":3,\"auditory\":\"спорт2\",\"teachers\":[],\"groups\":[8635574\n]}\n,\n{\"subject_id\":5318974,\"start_time\":1605684600,\"end_time\":1605690300,\"type\":0,\"number_pair\":2,\"auditory\":\"301б\",\"teachers\":[106],\"groups\":[8511166\n,8635576\n,8635574\n,8635572\n]}\n,\n{\"subject_id\":4809196,\"start_time\":1605690900,\"end_time\":1605696600,\"type\":0,\"number_pair\":3,\"auditory\":\"339\",\"teachers\":[2908],\"groups\":[8511166\n,8635572\n,8635574\n,8635576\n]}\n,\n{\"subject_id\":5306218,\"start_time\":1605697800,\"end_time\":1605703500,\"type\":0,\"number_pair\":4,\"auditory\":\"365\",\"teachers\":[5306219],\"groups\":[8635576\n,8635572\n,8635574\n,8511166\n]}\n,\n{\"subject_id\":6963436,\"start_time\":1605704100,\"end_time\":1605709800,\"type\":0,\"number_pair\":5,\"auditory\":\"365\",\"teachers\":[84],\"groups\":[8635574\n,8635572\n,8635576\n,8511166\n]}\n,\n{\"subject_id\":4809154,\"start_time\":1605764700,\"end_time\":1605770400,\"type\":0,\"number_pair\":1,\"auditory\":\"141\",\"teachers\":[172],\"groups\":[8635572\n,8635576\n,8511166\n,8635574\n]}\n,\n{\"subject_id\":5304362,\"start_time\":1605771000,\"end_time\":1605776700,\"type\":0,\"number_pair\":2,\"auditory\":\"365\",\"teachers\":[372],\"groups\":[8635572\n,8635574\n,8511166\n,8635576\n]}\n,\n{\"subject_id\":4809160,\"start_time\":1605777300,\"end_time\":1605783000,\"type\":0,\"number_pair\":3,\"auditory\":\"365\",\"teachers\":[326],\"groups\":[8635576\n,8511166\n,8635574\n,8635572\n]}\n,\n{\"subject_id\":5318974,\"start_time\":1605784200,\"end_time\":1605789900,\"type\":0,\"number_pair\":4,\"auditory\":\"365\",\"teachers\":[106],\"groups\":[8635574\n,8635576\n,8511166\n,8635572\n]}\n,\n{\"subject_id\":5304362,\"start_time\":1606136100,\"end_time\":1606141800,\"type\":10,\"number_pair\":5,\"auditory\":\"365\",\"teachers\":[3099669],\"groups\":[8635574\n]}\n,\n{\"subject_id\":5304362,\"start_time\":1606142400,\"end_time\":1606148100,\"type\":10,\"number_pair\":6,\"auditory\":\"365\",\"teachers\":[3099669],\"groups\":[8635574\n]}\n,\n{\"subject_id\":8051836,\"start_time\":1606209300,\"end_time\":1606215000,\"type\":10,\"number_pair\":3,\"auditory\":\"спорт2\",\"teachers\":[],\"groups\":[8635574\n]}\n,\n{\"subject_id\":4809196,\"start_time\":1606295700,\"end_time\":1606301400,\"type\":0,\"number_pair\":3,\"auditory\":\"339\",\"teachers\":[2908],\"groups\":[8635574\n,8511166\n,8635572\n,8635576\n]}\n,\n{\"subject_id\":5306218,\"start_time\":1606302600,\"end_time\":1606308300,\"type\":0,\"number_pair\":4,\"auditory\":\"365\",\"teachers\":[5306219],\"groups\":[8635572\n,8635576\n,8511166\n,8635574\n]}\n,\n{\"subject_id\":6963436,\"start_time\":1606308900,\"end_time\":1606314600,\"type\":0,\"number_pair\":5,\"auditory\":\"365\",\"teachers\":[84],\"groups\":[8511166\n,8635572\n,8635576\n,8635574\n]}\n,\n{\"subject_id\":4809154,\"start_time\":1606369500,\"end_time\":1606375200,\"type\":0,\"number_pair\":1,\"auditory\":\"140\",\"teachers\":[172],\"groups\":[8635576\n,8635572\n,8635574\n,8511166\n]}\n,\n{\"subject_id\":5304362,\"start_time\":1606375800,\"end_time\":1606381500,\"type\":0,\"number_pair\":2,\"auditory\":\"365\",\"teachers\":[372],\"groups\":[8635576\n,8635572\n,8635574\n,8511166\n]}\n,\n{\"subject_id\":4809160,\"start_time\":1606382100,\"end_time\":1606387800,\"type\":0,\"number_pair\":3,\"auditory\":\"365\",\"teachers\":[326],\"groups\":[8511166\n,8635574\n,8635572\n,8635576\n]}\n,\n{\"subject_id\":5318974,\"start_time\":1606389000,\"end_time\":1606394700,\"type\":0,\"number_pair\":4,\"auditory\":\"365\",\"teachers\":[106],\"groups\":[8635572\n,8635576\n,8635574\n,8511166\n]}\n,\n{\"subject_id\":8051836,\"start_time\":1606814100,\"end_time\":1606819800,\"type\":10,\"number_pair\":3,\"auditory\":\"спорт2\",\"teachers\":[],\"groups\":[8635574\n]}\n,\n{\"subject_id\":4809196,\"start_time\":1606900500,\"end_time\":1606906200,\"type\":0,\"number_pair\":3,\"auditory\":\"339\",\"teachers\":[2908],\"groups\":[8511166\n,8635576\n,8635574\n,8635572\n]}\n,\n{\"subject_id\":5306218,\"start_time\":1606907400,\"end_time\":1606913100,\"type\":0,\"number_pair\":4,\"auditory\":\"365\",\"teachers\":[5306219],\"groups\":[8511166\n,8635576\n,8635574\n,8635572\n]}\n,\n{\"subject_id\":6963436,\"start_time\":1606913700,\"end_time\":1606919400,\"type\":0,\"number_pair\":5,\"auditory\":\"365\",\"teachers\":[84],\"groups\":[8635572\n,8635574\n,8635576\n,8511166\n]}\n,\n{\"subject_id\":4809154,\"start_time\":1606974300,\"end_time\":1606980000,\"type\":0,\"number_pair\":1,\"auditory\":\"141\",\"teachers\":[172],\"groups\":[8635576\n,8635574\n,8511166\n,8635572\n]}\n,\n{\"subject_id\":4809154,\"start_time\":1606980600,\"end_time\":1606986300,\"type\":0,\"number_pair\":2,\"auditory\":\"140\",\"teachers\":[172],\"groups\":[8635572\n,8635574\n,8635576\n,8511166\n]}\n,\n{\"subject_id\":4809160,\"start_time\":1606986900,\"end_time\":1606992600,\"type\":0,\"number_pair\":3,\"auditory\":\"365\",\"teachers\":[326],\"groups\":[8511166\n,8635572\n,8635576\n,8635574\n]}\n,\n{\"subject_id\":5318974,\"start_time\":1606993800,\"end_time\":1606999500,\"type\":0,\"number_pair\":4,\"auditory\":\"365\",\"teachers\":[106],\"groups\":[8635572\n,8511166\n,8635574\n,8635576\n]}\n,\n{\"subject_id\":8051836,\"start_time\":1607418900,\"end_time\":1607424600,\"type\":10,\"number_pair\":3,\"auditory\":\"спорт2\",\"teachers\":[],\"groups\":[8635574\n]}\n,\n{\"subject_id\":4809196,\"start_time\":1607505300,\"end_time\":1607511000,\"type\":0,\"number_pair\":3,\"auditory\":\"339\",\"teachers\":[2908],\"groups\":[8511166\n,8635574\n,8635576\n,8635572\n]}\n,\n{\"subject_id\":5306218,\"start_time\":1607512200,\"end_time\":1607517900,\"type\":0,\"number_pair\":4,\"auditory\":\"365\",\"teachers\":[5306219],\"groups\":[8511166\n,8635576\n,8635572\n,8635574\n]}\n,\n{\"subject_id\":6963436,\"start_time\":1607518500,\"end_time\":1607524200,\"type\":0,\"number_pair\":5,\"auditory\":\"365\",\"teachers\":[84],\"groups\":[8511166\n,8635576\n,8635574\n,8635572\n]}\n,\n{\"subject_id\":4809154,\"start_time\":1607579100,\"end_time\":1607584800,\"type\":0,\"number_pair\":1,\"auditory\":\"140\",\"teachers\":[172],\"groups\":[8635576\n,8635572\n,8635574\n,8511166\n]}\n,\n{\"subject_id\":4809160,\"start_time\":1607585400,\"end_time\":1607591100,\"type\":0,\"number_pair\":2,\"auditory\":\"509і\",\"teachers\":[326],\"groups\":[8511166\n,8635572\n,8635576\n,8635574\n]}\n,\n{\"subject_id\":4809160,\"start_time\":1607591700,\"end_time\":1607597400,\"type\":0,\"number_pair\":3,\"auditory\":\"365\",\"teachers\":[326],\"groups\":[8511166\n,8635576\n,8635572\n,8635574\n]}\n,\n{\"subject_id\":5318974,\"start_time\":1607598600,\"end_time\":1607604300,\"type\":0,\"number_pair\":4,\"auditory\":\"365\",\"teachers\":[106],\"groups\":[8635574\n,8511166\n,8635576\n,8635572\n]}\n,\n{\"subject_id\":8051836,\"start_time\":1608023700,\"end_time\":1608029400,\"type\":10,\"number_pair\":3,\"auditory\":\"спорт2\",\"teachers\":[],\"groups\":[8635574\n]}\n,\n{\"subject_id\":4809196,\"start_time\":1608110100,\"end_time\":1608115800,\"type\":0,\"number_pair\":3,\"auditory\":\"339\",\"teachers\":[2908],\"groups\":[8511166\n,8635572\n,8635576\n,8635574\n]}\n,\n{\"subject_id\":5306218,\"start_time\":1608117000,\"end_time\":1608122700,\"type\":0,\"number_pair\":4,\"auditory\":\"141\",\"teachers\":[5306219],\"groups\":[8635574\n,8511166\n,8635576\n,8635572\n]}\n,\n{\"subject_id\":6963436,\"start_time\":1608123300,\"end_time\":1608129000,\"type\":0,\"number_pair\":5,\"auditory\":\"141\",\"teachers\":[84],\"groups\":[8635574\n,8511166\n,8635576\n,8635572\n]}\n,\n{\"subject_id\":4809154,\"start_time\":1608183900,\"end_time\":1608189600,\"type\":0,\"number_pair\":1,\"auditory\":\"140\",\"teachers\":[172],\"groups\":[8511166\n,8635576\n,8635574\n,8635572\n]}\n,\n{\"subject_id\":4809160,\"start_time\":1608190200,\"end_time\":1608195900,\"type\":0,\"number_pair\":2,\"auditory\":\"513і\",\"teachers\":[326],\"groups\":[8635574\n,8511166\n,8635572\n,8635576\n]}\n,\n{\"subject_id\":4809160,\"start_time\":1608196500,\"end_time\":1608202200,\"type\":0,\"number_pair\":3,\"auditory\":\"513і\",\"teachers\":[326],\"groups\":[8635574\n,8635572\n,8635576\n,8511166\n]}\n,\n{\"subject_id\":5318974,\"start_time\":1608203400,\"end_time\":1608209100,\"type\":0,\"number_pair\":4,\"auditory\":\"513і\",\"teachers\":[106],\"groups\":[8635576\n,8511166\n,8635572\n,8635574\n]}\n,\n{\"subject_id\":5304362,\"start_time\":1608555300,\"end_time\":1608561000,\"type\":10,\"number_pair\":5,\"auditory\":\"285\",\"teachers\":[3099669],\"groups\":[8635574\n]}\n,\n{\"subject_id\":5304362,\"start_time\":1608561600,\"end_time\":1608567300,\"type\":10,\"number_pair\":6,\"auditory\":\"285\",\"teachers\":[3099669],\"groups\":[8635574\n]}\n,\n{\"subject_id\":8051836,\"start_time\":1608628500,\"end_time\":1608634200,\"type\":10,\"number_pair\":3,\"auditory\":\"спорт2\",\"teachers\":[],\"groups\":[8635574\n]}\n,\n{\"subject_id\":4809196,\"start_time\":1608714900,\"end_time\":1608720600,\"type\":0,\"number_pair\":3,\"auditory\":\"339\",\"teachers\":[2908],\"groups\":[8511166\n,8635572\n,8635576\n,8635574\n]}\n,\n{\"subject_id\":5306218,\"start_time\":1608721800,\"end_time\":1608727500,\"type\":0,\"number_pair\":4,\"auditory\":\"141\",\"teachers\":[5306219],\"groups\":[8635574\n,8511166\n,8635576\n,8635572\n]}\n,\n{\"subject_id\":6963436,\"start_time\":1608728100,\"end_time\":1608733800,\"type\":0,\"number_pair\":5,\"auditory\":\"141\",\"teachers\":[84],\"groups\":[8635574\n,8511166\n,8635576\n,8635572\n]}\n,\n{\"subject_id\":4809154,\"start_time\":1608788700,\"end_time\":1608794400,\"type\":0,\"number_pair\":1,\"auditory\":\"140\",\"teachers\":[172],\"groups\":[8635576\n,8635572\n,8511166\n,8635574\n]}\n,\n{\"subject_id\":4809154,\"start_time\":1608795000,\"end_time\":1608800700,\"type\":0,\"number_pair\":2,\"auditory\":\"140\",\"teachers\":[172],\"groups\":[8511166\n,8635576\n,8635572\n,8635574\n]}\n,\n{\"subject_id\":4809160,\"start_time\":1608801300,\"end_time\":1608807000,\"type\":0,\"number_pair\":3,\"auditory\":\"513і\",\"teachers\":[326],\"groups\":[8635576\n,8511166\n,8635572\n,8635574\n]}\n,\n{\"subject_id\":5304362,\"start_time\":1609160100,\"end_time\":1609165800,\"type\":10,\"number_pair\":5,\"auditory\":\"285\",\"teachers\":[3099669],\"groups\":[8635574\n]}\n,\n{\"subject_id\":5304362,\"start_time\":1609166400,\"end_time\":1609172100,\"type\":10,\"number_pair\":6,\"auditory\":\"285\",\"teachers\":[3099669],\"groups\":[8635574\n]}]\n,\"groups\":[\n{\"id\":8635574,\"name\":\"ІПЗм-20-3\"}\n,\n{\"id\":8511166,\"name\":\"ІПЗм-20-1\"}\n,\n{\"id\":8635576,\"name\":\"ІПЗм-20-4\"}\n,\n{\"id\":8635572,\"name\":\"ІПЗм-20-2\"}\n]\n,\"teachers\":[\n{\"id\":2908,\"full_name\":\"Голян Вiра Володимирiвна\",\"short_name\":\"Голян В. В.\"}\n,\n{\"id\":326,\"full_name\":\"Ревенчук Ілона Анатоліївна\",\"short_name\":\"Ревенчук І. А.\"}\n,\n{\"id\":3099669,\"full_name\":\"Сковороднікова Вікторія Валеріївна\",\"short_name\":\"Сковороднікова В. В.\"}\n,\n{\"id\":84,\"full_name\":\"Каук Віктор Іванович\",\"short_name\":\"Каук В. І.\"}\n,\n{\"id\":106,\"full_name\":\"Єрохін Андрій Леонідович\",\"short_name\":\"Єрохін А. Л.\"}\n,\n{\"id\":172,\"full_name\":\"Шубін Ігор Юрійович\",\"short_name\":\"Шубін І. Ю.\"}\n,\n{\"id\":5306219,\"full_name\":\"Власенко Лариса  Андріївна\",\"short_name\":\"Власенко Л. А.\"}\n,\n{\"id\":372,\"full_name\":\"Дудар Зоя Володимирівна\",\"short_name\":\"Дудар З. В.\"}\n]\n,\"subjects\": [\n{\"id\":4809154,\"brief\":\"ФМІПЗ\",\"title\":\"Формальні методи інженерії програмного забезпечення\",\"hours\":[\n{\"type\":0,\"val\":30,\"teachers\":[\n172\n]}\n]},\n{\"id\":4809160,\"brief\":\"ІПвІПЗ\",\"title\":\"Інноваційне підприємництво в індустрії програмного забезпечення\",\"hours\":[\n{\"type\":0,\"val\":30,\"teachers\":[\n326\n]}\n]},\n{\"id\":4809196,\"brief\":\"ТРПС\",\"title\":\"Технології розробки програмних систем\",\"hours\":[\n{\"type\":0,\"val\":30,\"teachers\":[\n2908\n]}\n]},\n{\"id\":5304362,\"brief\":\"ВтаАП\",\"title\":\"Винахідництво та авторське право\",\"hours\":[\n{\"type\":0,\"val\":18,\"teachers\":[\n372\n]}\n,\n{\"type\":10,\"val\":12,\"teachers\":[\n3099669\n]}\n]},\n{\"id\":5306218,\"brief\":\"ТОПС\",\"title\":\"Теорія оптимізації програмних систем\",\"hours\":[\n{\"type\":0,\"val\":24,\"teachers\":[\n5306219\n]}\n]},\n{\"id\":5318974,\"brief\":\"ОНДАП\",\"title\":\"Основи наукових досліджень, організація науки та авторське право\",\"hours\":[\n{\"type\":0,\"val\":24,\"teachers\":[\n106\n]}\n]},\n{\"id\":6963436,\"brief\":\"МКМ\",\"title\":\"Методологія конструктивного мислення для наукових досліджень\",\"hours\":[\n{\"type\":0,\"val\":24,\"teachers\":[\n84\n]}\n]},\n{\"id\":8051836,\"brief\":\"ФВ\",\"title\":\"Фізичне виховання (за рахунок вільного часу студентів)\",\"hours\":[\n{\"type\":10,\"val\":34,\"teachers\":[\n]}\n]}\n]\n,\"types\": [\n{\"id\":0,\"short_name\":\"Лк\",\"full_name\":\"Лекція\",\n\"id_base\":0, \"type\":\"lecture\"}\n,\n{\"id\":1,\"short_name\":\"У.Лк (1)\",\"full_name\":\"Уст. Лекція (1)\",\n\"id_base\":0, \"type\":\"lecture\"}\n,\n{\"id\":2,\"short_name\":\"У.Лк\",\"full_name\":\"Уст. Лекція\",\n\"id_base\":0, \"type\":\"lecture\"}\n,\n{\"id\":10,\"short_name\":\"Пз\",\"full_name\":\"Практичне заняття\",\n\"id_base\":10, \"type\":\"practice\"}\n,\n{\"id\":12,\"short_name\":\"У.Пз\",\"full_name\":\"Уст. Практичне заняття\",\n\"id_base\":10, \"type\":\"practice\"}\n,\n{\"id\":20,\"short_name\":\"Лб\",\"full_name\":\"Лабораторна робота\",\n\"id_base\":20, \"type\":\"laboratory\"}\n,\n{\"id\":21,\"short_name\":\"Лб\",\"full_name\":\"Лабораторна ІОЦ\",\n\"id_base\":20, \"type\":\"laboratory\"}\n,\n{\"id\":22,\"short_name\":\"Лб\",\"full_name\":\"Лабораторна кафедри\",\n\"id_base\":20, \"type\":\"laboratory\"}\n,\n{\"id\":23,\"short_name\":\"У.Лб\",\"full_name\":\"Уст. Лабораторна ІОЦ\",\n\"id_base\":20, \"type\":\"laboratory\"}\n,\n{\"id\":24,\"short_name\":\"У.Лб\",\"full_name\":\"Уст. Лабораторна кафедри\",\n\"id_base\":20, \"type\":\"laboratory\"}\n,\n{\"id\":30,\"short_name\":\"Конс\",\"full_name\":\"Консультація\",\n\"id_base\":30, \"type\":\"consultation\"}\n,\n{\"id\":40,\"short_name\":\"Зал\",\"full_name\":\"Залік\",\n\"id_base\":40, \"type\":\"test\"}\n,\n{\"id\":41,\"short_name\":\"дзал\",\"full_name\":\"ЗалікД\",\n\"id_base\":40, \"type\":\"test\"}\n,\n{\"id\":50,\"short_name\":\"Екз\",\"full_name\":\"Екзамен\",\n\"id_base\":50, \"type\":\"exam\"}\n,\n{\"id\":51,\"short_name\":\"ЕкзП\",\"full_name\":\"ЕкзаменП\",\n\"id_base\":50, \"type\":\"exam\"}\n,\n{\"id\":52,\"short_name\":\"ЕкзУ\",\"full_name\":\"ЕкзаменУ\",\n\"id_base\":50, \"type\":\"exam\"}\n,\n{\"id\":53,\"short_name\":\"ІспКомб\",\"full_name\":\"Іспит комбінований\",\n\"id_base\":50, \"type\":\"exam\"}\n,\n{\"id\":54,\"short_name\":\"ІспТест\",\"full_name\":\"Іспит тестовий\",\n\"id_base\":50, \"type\":\"exam\"}\n,\n{\"id\":55,\"short_name\":\"мод\",\"full_name\":\"Модуль\",\n\"id_base\":50, \"type\":\"exam\"}\n,\n{\"id\":60,\"short_name\":\"КП/КР\",\"full_name\":\"КП/КР\",\n\"id_base\":60, \"type\":\"course_work\"}\n]\n}\n";
        }
    }
}