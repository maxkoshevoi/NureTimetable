using NureTimetable.Core.Models.Consts;
using NureTimetable.Core.Models.InterplatformCommunication;
using NureTimetable.Models.Consts;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;
using Settings = NureTimetable.Core.Models.Settings;

namespace NureTimetable.UI.Themes
{
    public static class ThemeHelper
    {
        public static bool SetAppTheme(Settings.AppTheme selectedTheme)
        {
            if (selectedTheme == Settings.AppTheme.FollowSystem)
            {
                selectedTheme = (Settings.AppTheme)App.Current.RequestedTheme;
            }

            ResourceDictionary theme = selectedTheme switch
            {
                Settings.AppTheme.Dark => new DarkTheme(),
                Settings.AppTheme.Light => new LightTheme(),
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

            MainThread.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Send(Application.Current, MessageTypes.ThemeChanged, selectedTheme);
            });

            return true;
        }
    }
}
