using Android.Graphics;
using Android.OS;
using Android.Views;
using NureTimetable.Core.Models.InterplatformCommunication;
using NureTimetable.Droid.Dependences;
using Xamarin.Essentials;
using NureTimetable.Core.Extensions;
using System;

[assembly: Xamarin.Forms.Dependency(typeof(BarStyleManager))]
namespace NureTimetable.Droid.Dependences
{
    public class BarStyleManager : IBarStyleManager
    {
        public void SetStatusBarColor(string hexColor)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
                return;
            }

            Color color = Color.ParseColor(hexColor);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var currentWindow = GetCurrentWindow();
                SetBarAppearance(currentWindow, statusBarLight: color == Color.White);
                currentWindow.SetStatusBarColor(color);
            });
        }

        public void SetNavigationBarColor(string hexColor)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
                return;
            }

            Color color = Color.ParseColor(hexColor);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var currentWindow = GetCurrentWindow();
                SetBarAppearance(currentWindow, navigationBarLight: color == Color.White);
                currentWindow.SetNavigationBarColor(color);
            });
        }

        private static void SetBarAppearance(Window currentWindow, bool? statusBarLight = null, bool? navigationBarLight = null)
        {
            StatusBarVisibility barAppearanceOld = 0;
            WindowInsetsControllerAppearance barAppearanceNew = 0;
            if ((int)Build.VERSION.SdkInt < 30)
            {
#pragma warning disable CS0618 // Type or member is obsolete. Using new API for Sdk 30+
                barAppearanceOld = currentWindow.DecorView.SystemUiVisibility;
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
                barAppearanceNew = (WindowInsetsControllerAppearance)currentWindow.InsetsController!.SystemBarsAppearance;
            }

            if (statusBarLight == true)
            {
                barAppearanceOld = barAppearanceOld.AddFlag((StatusBarVisibility)SystemUiFlags.LightStatusBar);
                barAppearanceNew = barAppearanceNew.AddFlag(WindowInsetsControllerAppearance.LightStatusBars);
            }
            else if (statusBarLight == false)
            {
                barAppearanceOld = barAppearanceOld.RemoveFlag((StatusBarVisibility)SystemUiFlags.LightStatusBar);
                barAppearanceNew = barAppearanceNew.RemoveFlag(WindowInsetsControllerAppearance.LightStatusBars);
            }
            if (navigationBarLight == true)
            {
                barAppearanceOld = barAppearanceOld.AddFlag((StatusBarVisibility)SystemUiFlags.LightNavigationBar);
                barAppearanceNew = barAppearanceNew.AddFlag(WindowInsetsControllerAppearance.LightNavigationBars);
            }
            else if (navigationBarLight == false)
            {
                barAppearanceOld = barAppearanceOld.RemoveFlag((StatusBarVisibility)SystemUiFlags.LightNavigationBar);
                barAppearanceNew = barAppearanceNew.RemoveFlag(WindowInsetsControllerAppearance.LightNavigationBars);
            }
            
            if ((int)Build.VERSION.SdkInt < 30)
            {
#pragma warning disable CS0618 // Type or member is obsolete. Using new API for Sdk 30+
                currentWindow.DecorView.SystemUiVisibility = barAppearanceOld;
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
                currentWindow.InsetsController?.SetSystemBarsAppearance((int)barAppearanceNew, (int)barAppearanceNew);
            }
        }

        private Window GetCurrentWindow()
        {
            Window window = Platform.CurrentActivity.Window ?? throw new NullReferenceException("window");
            window.ClearFlags(WindowManagerFlags.TranslucentStatus);
            window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
            return window;
        }
    }
}