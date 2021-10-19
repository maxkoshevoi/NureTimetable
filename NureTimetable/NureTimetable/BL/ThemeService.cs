using NureTimetable.Core.BL;
using NureTimetable.Core.Models.Consts;
using NureTimetable.Core.Models.InterplatformCommunication;
using NureTimetable.DAL;
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
        public static bool SetAppTheme()
        {
            AppTheme selectedTheme = SettingsRepository.Settings.Theme;

            UpdateNativeStyle(selectedTheme);
            UpdateAppStyle(selectedTheme);

            MessagingCenter.Send(Application.Current, MessageTypes.ThemeChanged, selectedTheme);
            return true;
        }

        private static bool UpdateAppStyle(AppTheme selectedTheme)
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
            if (resources.FirstOrDefault()?.GetType() == theme.GetType())
            {
                return false;
            }

            try
            {
                resources.Clear();
                resources.Add(theme);
            }
            catch (Exception ex)
            {
                ExceptionService.LogException(ex);
            }

            return true;
        }

        private static void UpdateNativeStyle(AppTheme selectedTheme)
        {
            NightModeStyle style = selectedTheme switch
            {
                AppTheme.Dark => NightModeStyle.Yes,
                AppTheme.Light => NightModeStyle.No,
                AppTheme.FollowSystem => NightModeStyle.FollowSystem,
                _ => throw new InvalidOperationException("Unsupported theme"),
            };

            var nightModeManager = DependencyService.Get<INightModeManager>();
            nightModeManager.DefaultNightMode = style;
        }
    }
}
