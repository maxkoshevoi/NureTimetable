using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Plugin.InAppBilling;
using Android.Content;
using Plugin.CurrentActivity;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using NureTimetable.Droid.BugFixes;

namespace NureTimetable.Droid
{
    [Activity(Label = "NureTimetable", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);
            Forms.Init(this, savedInstanceState);
            CrossCurrentActivity.Current.Activity = this;
            LoadApplication(new App());
            
            // Fix the keyboard so it doesn't overlap the grid icons above keyboard etc, and makes Android 5+ work as AdjustResize in Android 4
            Window.SetSoftInputMode(SoftInput.AdjustResize);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                // Bug in Android 5+, this is an adequate workaround
                AndroidBug5497WorkaroundForXamarinAndroid.assistActivity(this, WindowManager);
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            InAppBillingImplementation.HandleActivityResult(requestCode, resultCode, data);
        }
    }
}