using Microsoft.Maui.Graphics;
using System.Runtime.CompilerServices;

namespace NureTimetable.UI.Models.Consts
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

        // public static NavigationBarStyle NavigationBarStyle => Get<NavigationBarStyle>();

        public static Color PageBackgroundColor => GetColor();

        private static Color GetColor([CallerMemberName] string resourceName = "") => Get<Color>(resourceName);

        private static T Get<T>([CallerMemberName] string resourceName = "")
        {
            var value = (T)App.Current.Resources[resourceName];
            return value;
        }
    }
}
