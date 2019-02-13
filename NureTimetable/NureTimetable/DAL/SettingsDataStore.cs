using NureTimetable.Helpers;
using NureTimetable.Models.Consts;
using System;
using System.IO;

namespace NureTimetable.DAL
{
    public static class SettingsDataStore
    {
        public static DateTime? GetLastTimetableUpdate()
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

        public static void UpdateLastTimetableUpdate()
        {
            Serialisation.ToJsonFile(DateTime.Now, FilePath.LastTimetableUpdate);
        }
    }
}
