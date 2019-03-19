using NureTimetable.Helpers;
using NureTimetable.Models;
using NureTimetable.Models.Consts;
using NureTimetable.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NureTimetable.DAL
{
    public static class SettingsDataStore
    {
        #region Timetable Update Rights
        public static List<SavedGroup> CheckCistTimetableUpdateRights(params Group[] groupsToUpdate)
        {
            var allowedGroups = new List<SavedGroup>();
            if (groupsToUpdate == null)
            {
                return allowedGroups;
            }

            List<SavedGroup> savedGroups = GroupsDataStore.GetSaved();
            foreach (Group group in groupsToUpdate)
            {
                SavedGroup savedGroup = savedGroups.FirstOrDefault(g => g.ID == group.ID);
                if (savedGroup == null)
                {
                    // Cannot update timetable for group that is not saved
                    continue;
                }

                if (savedGroup.LastUpdated == null || (DateTime.Now.TimeOfDay.Hours >= 5 && DateTime.Now.TimeOfDay.Hours < 7))
                {
                    // Update allowed if never updated before
                    // Unlimited updates between 5 and 7 AM
                    allowedGroups.Add(savedGroup);
                    continue;
                }

                TimeSpan timeBeforeAnotherUpdate;
                if (savedGroup.LastUpdated.Value.TimeOfDay < Config.CistDailyTimetableUpdateTime)
                {
                    timeBeforeAnotherUpdate = Config.CistDailyTimetableUpdateTime - savedGroup.LastUpdated.Value.TimeOfDay;
                }
                else
                {
                    timeBeforeAnotherUpdate = TimeSpan.FromDays(1) - savedGroup.LastUpdated.Value.TimeOfDay + Config.CistDailyTimetableUpdateTime;
                }
                if (DateTime.Now - savedGroup.LastUpdated.Value > timeBeforeAnotherUpdate)
                {
                    allowedGroups.Add(savedGroup);
                    continue;
                }
#if DEBUG
                allowedGroups.Add(savedGroup);
#endif
            }
            return allowedGroups;
        }
        #endregion

        #region Lessons Info Update Rights
        public static List<Group> CheckCistLessonsInfoUpdateRights(params Group[] groupsToUpdate)
        {
            var allowedGroups = new List<Group>();
            if (groupsToUpdate == null)
            {
                return allowedGroups;
            }

            List<TimetableInfo> localTimetables = EventsDataStore.GetTimetableLocal(groupsToUpdate);
            foreach (Group group in groupsToUpdate)
            {
                TimetableInfo groupTimetable = localTimetables.FirstOrDefault(tt => tt.Group.ID == group.ID);
                if (groupTimetable == null)
                {
                    allowedGroups.Add(group);
                    continue;
                }
                if (groupTimetable.Lessons().Any(lesson => !groupTimetable.LessonsInfo.Exists(li => li.ShortName == lesson)) 
                    || groupTimetable.LessonsInfo.Exists(li => li.LastUpdated == null))
                {
                    // Allow update if at least one lesson never updated before
                    allowedGroups.Add(group);
                    continue;
                }
                if (groupTimetable.LessonsInfo.Exists(li => (DateTime.Now - li.LastUpdated) >= Config.CistLessonsInfoUpdateMinInterval))
                {
                    // Allow update if enough time passed
                    allowedGroups.Add(group);
                    continue;
                }
#if DEBUG
                allowedGroups.Add(group);
#endif
            }
            return allowedGroups;
        }
        #endregion

        #region All Groups Update Rights
        public static bool CheckCistAllGroupsUpdateRights()
        {
#if DEBUG
            return true;
#endif

            TimeSpan? timePass = DateTime.Now - GetLastCistAllGroupsUpdateTime();
            if (timePass != null && timePass <= Config.CistAllGroupsUpdateMinInterval)
            {
                return false;
            }
            return true;
        }

        private static DateTime? GetLastCistAllGroupsUpdateTime()
        {
            string filePath = FilePath.LastCistAllGroupsUpdate;
            if (!File.Exists(filePath))
            {
                return null;
            }

            DateTime lastTimetableUpdate = Serialisation.FromJsonFile<DateTime>(filePath);
            return lastTimetableUpdate;
        }

        public static void UpdateCistAllGroupsUpdateTime()
        {
            Serialisation.ToJsonFile(DateTime.Now, FilePath.LastCistAllGroupsUpdate);
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
