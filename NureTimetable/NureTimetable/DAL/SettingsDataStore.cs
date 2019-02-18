using NureTimetable.Helpers;
using NureTimetable.Models.Consts;
using System;
using System.IO;

namespace NureTimetable.DAL
{
    public static class SettingsDataStore
    {
        public static bool CheckGetDataFromCistRights()
        {
            TimeSpan? timePass = DateTime.Now - GetLastCistRequestTime();
            if (timePass != null && timePass <= Config.CistRequestMinInterval)
            {
                return false;
            }
            return true;
        }

        public static DateTime? GetLastCistRequestTime()
        {
#if DEBUG
return null;
#endif

            string filePath = FilePath.LastTimetableUpdate;
            if (!File.Exists(filePath))
            {
                return null;
            }

            DateTime lastTimetableUpdate = Serialisation.FromJsonFile<DateTime>(filePath);
            return lastTimetableUpdate;
        }

        public static void UpdateLastCistRequestTime()
        {
            Serialisation.ToJsonFile(DateTime.Now, FilePath.LastTimetableUpdate);
        }
    }
}
