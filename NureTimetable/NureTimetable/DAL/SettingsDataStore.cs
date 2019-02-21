using NureTimetable.Helpers;
using NureTimetable.Models;
using NureTimetable.Models.Consts;
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
            List<SavedGroup> allowedGroups = new List<SavedGroup>();
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

                if (savedGroup.LastUpdated == null)
                {
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
            }
            return allowedGroups;
        }
        #endregion

        #region All Groups Update Rights
        public static bool CheckCistAllGroupsUpdateRights()
        {
            TimeSpan? timePass = DateTime.Now - GetLastCistAllGroupsUpdateTime();
            if (timePass != null && timePass <= Config.CistAllGroupsUpdateMinInterval)
            {
                return false;
            }
            return true;
        }

        private static DateTime? GetLastCistAllGroupsUpdateTime()
        {
//#if DEBUG
//return null;
//#endif

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
    }
}
