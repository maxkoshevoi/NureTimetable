using Xamarin.Forms;

namespace NureTimetable.Models.Consts
{
    public static class ResourceManager
    {
        public static Color EventColor(Event e)
            => (Color) App.Current.Resources[ResourceNames.EventColor(e.Type)];
    }
}
