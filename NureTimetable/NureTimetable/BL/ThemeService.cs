using NureTimetable.Core.BL;
using NureTimetable.Core.Models.Consts;
using NureTimetable.Core.Models.InterplatformCommunication;
using NureTimetable.Models.Consts;
using NureTimetable.UI.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using AppTheme = NureTimetable.Core.Models.Settings.AppTheme;

namespace NureTimetable.BL
{
    public static class ThemeService
    {
        public static bool SetAppTheme(AppTheme selectedTheme)
        {
            if (selectedTheme == AppTheme.FollowSystem)
            {
                selectedTheme = (AppTheme)App.Current.RequestedTheme;
            }

            ResourceDictionary theme = selectedTheme switch
            {
                AppTheme.Dark => new DarkTheme(),
                AppTheme.Light => new LightTheme(),
                _ => throw new InvalidOperationException("Unsupported theme"),
            };

            ICollection<ResourceDictionary> resources = Application.Current.Resources.MergedDictionaries;
            if (resources == null || resources.FirstOrDefault()?.GetType() == theme.GetType())
            {
                return false;
            }
            resources.Clear();
            try
            {
                resources.Add(theme);
            }
            catch (Exception ex)
            {
                ExceptionService.LogException(ex);
            }

            var statusBarManager = DependencyService.Get<IBarStyleManager>();
            statusBarManager.SetStatusBarColor(ResourceManager.StatusBarColor.ToHex());
            statusBarManager.SetNavigationBarColor(ResourceManager.NavigationBarColor.ToHex());

            MessagingCenter.Send(Application.Current, MessageTypes.ThemeChanged, selectedTheme);
            return true;
        }
    }
}
