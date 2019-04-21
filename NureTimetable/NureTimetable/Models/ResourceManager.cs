using Xamarin.Forms;

namespace NureTimetable.Models.Consts
{
    public static class ResourceManager
    {
        public static Color EventColor(string typeName)
        {
            string key = $"{typeName.ToLower()}Color";
            if (!App.Current.Resources.ContainsKey(key))
            {
                key = "defaultColor";
            }
            Color typeColor = (Color)App.Current.Resources[key];
            return typeColor;
        }
    }
}
