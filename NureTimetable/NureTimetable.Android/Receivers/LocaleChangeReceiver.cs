using Android.App;
using Android.Content;
using Java.Util;
using NureTimetable.Core.Models.Settings;
using NureTimetable.DAL;
using System.Globalization;
using Xamarin.CommunityToolkit.Helpers;

namespace NureTimetable.Droid.Receivers
{
    [BroadcastReceiver(Enabled = true)]
    [IntentFilter(new[] { Intent.ActionLocaleChanged })]
    public class LocaleChangeReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context? context, Intent? intent)
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(Locale.Default.Language);
            if (SettingsRepository.Settings.Language == AppLanguage.FollowSystem)
            {
                LocalizationResourceManager.Current.CurrentCulture = CultureInfo.CurrentCulture;
            }
        }
    }
}