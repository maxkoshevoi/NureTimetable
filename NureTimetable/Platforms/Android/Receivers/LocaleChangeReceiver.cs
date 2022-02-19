﻿using Android.App;
using Android.Content;

namespace NureTimetable.Platforms.Android.Receivers;

[BroadcastReceiver(Enabled = true, Exported = true)]
[IntentFilter(new[] { Intent.ActionLocaleChanged })]
public class LocaleChangeReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(Java.Util.Locale.Default.Language);
        if (SettingsRepository.Settings.Language == AppLanguage.FollowSystem)
        {
            LocalizationResourceManager.Current.CurrentCulture = CultureInfo.CurrentCulture;
        }
    }
}
