﻿using NureTimetable.Core.Extensions;
using NureTimetable.Core.Models.Consts;
using NureTimetable.DAL.Helpers;
using NureTimetable.DAL.Models.Consts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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
        public static Local.TimetableInfo GetEvents(Local.SavedEntity entity, bool tryUpdate = false, DateTime? dateStart = null, DateTime? dateEnd = null)
        {
            Local.TimetableInfo timetable;
            if (tryUpdate)
            {
                if (dateStart == null || dateEnd == null)
                {
                    throw new ArgumentNullException($"{nameof(dateStart)} and {nameof(dateEnd)} must be set");
                }

                timetable = GetTimetableFromCist(entity, dateStart.Value, dateEnd.Value);
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
            => GetTimetableLocal(new[] { entity }).FirstOrDefault();

        public static List<Local.TimetableInfo> GetTimetableLocal(params Local.SavedEntity[] entities)
        {
            var timetables = new List<Local.TimetableInfo>();
            if (entities == null)
            {
                return timetables;
            }
            foreach (Local.SavedEntity entity in entities)
            {
                Local.TimetableInfo timetableInfo = Serialisation.FromJsonFile<Local.TimetableInfo>(FilePath.SavedTimetable(entity.Type, entity.ID));
                if (timetableInfo == null)
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
            Device.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Send(Application.Current, MessageTypes.LessonSettingsChanged, entity);
            });
        }
        #endregion
        #endregion

        #region Cist
        public static Local.TimetableInfo GetTimetableFromCist(Local.SavedEntity entity, DateTime dateStart, DateTime dateEnd)
        {
            if (SettingsRepository.CheckCistTimetableUpdateRights(entity).Count == 0)
            {
                return null;
            }

            using (var client = new WebClient
            {
                Encoding = Encoding.GetEncoding("Windows-1251")
            })
            {
                try
                {
                    Local.TimetableInfo timetable = GetTimetableLocal(entity);

                    // Getting events
                    Uri uri = new Uri(Urls.CistEntityTimetableUrl(entity.Type, entity.ID, dateStart, dateEnd));
                    string responseStr = client.DownloadString(uri);
                    responseStr = responseStr.Replace("&amp;", "&");
                    responseStr = responseStr.Replace("\"events\":[\n]}]", "\"events\": []");
                    Cist.Timetable cistTimetable = Serialisation.FromJson<Cist.Timetable>(responseStr);

                    // Updating timetable information
                    if (timetable == null)
                    {
                        timetable = new Local.TimetableInfo(entity);
                    }
                    timetable.Events = cistTimetable.Events.Select(ev =>
                    {
                        Local.Event localEvent = MapConfig.Map<Cist.Event, Local.Event>(ev);
                        localEvent.Lesson = MapConfig.Map<Cist.Lesson, Local.Lesson>(cistTimetable.Lessons.First(l => l.Id == ev.LessonId));
                        localEvent.Type = MapConfig.Map<Cist.EventType, Local.EventType>(cistTimetable.EventTypes.First(et => et.Id == ev.TypeId));
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
                    }).ToList();

                    // Updating LastUpdated for saved groups 
                    List<Local.SavedEntity> AllSavedEntities = UniversityEntitiesRepository.GetSaved();
                    foreach (Local.SavedEntity savedEntity in AllSavedEntities.Where(e => e == entity))
                    {
                        savedEntity.LastUpdated = DateTime.Now;
                    }
                    UniversityEntitiesRepository.UpdateSaved(AllSavedEntities);

                    // Saving timetables
                    UpdateTimetableLocal(timetable);
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        MessagingCenter.Send(Application.Current, MessageTypes.TimetableUpdated, entity);
                    });

                    return timetable;
                }
                catch (Exception ex)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        MessagingCenter.Send(Application.Current, MessageTypes.ExceptionOccurred, ex);
                    });
                }
            }
            return null;
        }
        #endregion
    }
}