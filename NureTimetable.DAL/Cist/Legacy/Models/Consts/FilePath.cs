namespace NureTimetable.DAL.Cist.Legacy.Models.Consts;

[Obsolete("", true)]
static class FilePath
{
    public static string LocalStorage =>
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    public static string SavedTimetable(int groupID) =>
        Path.Combine(LocalStorage, $"timetable_{groupID}.json");

    #region Groups
    public static string SavedGroupsList =>
        Path.Combine(LocalStorage, "groups_saved.json");

    public static string SelectedGroup =>
        Path.Combine(LocalStorage, "group_selected.json");

    public static string AllGroupsList =>
        Path.Combine(LocalStorage, "groups_all.json");

    public static string LastCistAllGroupsUpdate =>
        Path.Combine(LocalStorage, "last_all_groups_update.json");
    #endregion

    public static string AppSettings =>
        Path.Combine(LocalStorage, "app_settings.json");
}
