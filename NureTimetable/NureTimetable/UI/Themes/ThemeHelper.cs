using NureTimetable.Core.Models.InterplatformCommunication;
using NureTimetable.Models.Consts;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace NureTimetable.UI.Themes
{
    public static class ThemeHelper
    {
        public static bool SetAppTheme(OSAppTheme selectedTheme)
        {
            ResourceDictionary theme = selectedTheme switch
            {
                OSAppTheme.Dark => new DarkTheme(),
                OSAppTheme.Light => new LightTheme(),
                _ => throw new InvalidOperationException("Unsupported theme"),
            };

            ICollection<ResourceDictionary> resources = Application.Current.Resources.MergedDictionaries;
            if (resources is null || resources.FirstOrDefault()?.GetType() == theme.GetType())
            {
                return false;
            }
            resources.Clear();
            resources.Add(theme);

            var statusBarManager = DependencyService.Get<IBarStyleManager>();
            statusBarManager.SetStatusBarColor(ResourceManager.StatusBarColor.ToHex());
            statusBarManager.SetNavigationBarColor(ResourceManager.NavigationBarColor.ToHex());
            return true;
        }
    }
}
