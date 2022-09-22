using Android.App;
using Android.Content;
using Java.Util;
using NureTimetable.BL;
using System.Globalization;
using Xamarin.CommunityToolkit.Helpers;

#nullable enable

namespace NureTimetable.Droid.Receivers
{
    [BroadcastReceiver(Enabled = true, Exported = true)]
    [IntentFilter(new[] { Intent.ActionLocaleChanged })]
    public class LocaleChangeReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context? context, Intent? intent)
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(Locale.Default.Language);
            LocalizationResourceManager.Current.CurrentCulture = LocalizationService.GetPreferredCulture();
        }
    }
}