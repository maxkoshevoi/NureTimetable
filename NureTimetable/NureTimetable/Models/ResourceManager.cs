using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace NureTimetable.Models.Consts
{
    public static class ResourceManager
    {
        public static Color EventColor(string typeName)
        {
            string key = $"{typeName.ToLower()}Color";
            if (App.Current.Resources.TryGetValue(key, out object colorValue))
            {
                return (Color)colorValue;
            }
            else
            {
                Color typeColor = GetColor("defaultColor");
                return typeColor;
            }
        }

        public static Color StatusBarColor => GetColor();

        public static Color NavigationBarColor => GetColor();

        public static Color PageBackgroundColor => GetColor();

        private static Color GetColor([CallerMemberName] string resourceName = "")
        {
            var color = (Color)App.Current.Resources[resourceName];
            return color;
        }
    }
}
