namespace NureTimetable.UI.ViewModels.Menu
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
