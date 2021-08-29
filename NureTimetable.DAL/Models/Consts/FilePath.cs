using NureTimetable.DAL.Models.Local;

namespace NureTimetable.DAL.Models.Consts
{
    public static class FilePath
    {
        public static string LocalStorage =>
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData); // FileSystem.AppDataDirectory

        public static string SavedTimetable(TimetableEntityType type, long entityId) =>
            Path.Combine(LocalStorage, $"timetable_{(int)type}_{entityId}.json");

        #region Groups
        public static string SavedEntitiesList =>
            Path.Combine(LocalStorage, "entities_saved.json");

        public static string UniversityEntities =>
            Path.Combine(LocalStorage, "university_entities.json");
        #endregion
    }
}
