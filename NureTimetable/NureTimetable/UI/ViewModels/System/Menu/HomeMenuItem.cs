namespace NureTimetable.UI.ViewModels.System.Menu
{
    public enum MenuItemType
    {
        Timetable,
        About,
        Donate
    }

    public class HomeMenuItem
    {
        public MenuItemType Id { get; set; }

        public string Title { get; set; }
    }
}
