using NureTimetable.DAL.Models;
using System;
using System.IO;

namespace NureTimetable.DAL.Consts
{
    public static class FilePath
    {
        public static string LocalStorage =>
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData); // FileSystem.AppDataDirectory

        public static string SavedTimetable(TimetableEntityType type, long entityId) =>
            Path.Combine(LocalStorage, $"timetable_{(int)type}_{entityId}.json");

        public static string SavedEntitiesList => Path.Combine(LocalStorage, "entities_saved.json");

        public static string UniversityEntities => Path.Combine(LocalStorage, "university_entities.json");

        public static string MoodleUser => Path.Combine(LocalStorage, "moodle_user.json");
    }
}
