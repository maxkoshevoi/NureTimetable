using System;
using System.IO;

namespace NureTimetable.Models.Consts
{
    public static class FilePath
    {
        public static string LocalStorage =>
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        public static string SavedTimetable(int groupID) =>
            Path.Combine(LocalStorage, $"timetable_{groupID}.json");

        public static string SavedGroupsList =>
            Path.Combine(LocalStorage, "groups_saved.json");

        public static string SelectedGroup =>
            Path.Combine(LocalStorage, "group_selected.json");

        public static string AllGroupsList =>
            Path.Combine(LocalStorage, "groups_all.json");

        public static string LastCistAllGroupsUpdate =>
            Path.Combine(LocalStorage, "last_all_groups_update.json");

        public static string AppSettings =>
            Path.Combine(LocalStorage, "app_settings.json");
    }
}
