using Foundation;
using NureTimetable.Core.Models.InterplatformCommunication;
using NureTimetable.iOS.Dependences;
using UIKit;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: Dependency(typeof(BarStyleManager))]
namespace NureTimetable.iOS.Dependences
{
    public class BarStyleManager : IBarStyleManager
    {
        public void SetNavigationBarColor(string hexColor)
        {
            // Not applicable to iOS, i think
        }

        public void SetStatusBarColor(string hexColor)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Color color = Color.FromHex(hexColor);
                if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
                {
                    UIView statusBar = new(UIApplication.SharedApplication.KeyWindow.WindowScene.StatusBarManager.StatusBarFrame)
                    {
                        BackgroundColor = color.ToUIColor()
                    };
                    UIApplication.SharedApplication.KeyWindow.AddSubview(statusBar);
                }
                else
                {
                    UIView statusBar = UIApplication.SharedApplication.ValueForKey(new NSString("statusBar")) as UIView;
                    if (statusBar.RespondsToSelector(new ObjCRuntime.Selector("setBackgroundColor:")))
                    {
                        statusBar.BackgroundColor = color.ToUIColor();
                    }
                }

                UIStatusBarStyle style = color == Color.White ? UIStatusBarStyle.DarkContent : UIStatusBarStyle.LightContent;
                UIApplication.SharedApplication.SetStatusBarStyle(style, false);
                GetCurrentViewController().SetNeedsStatusBarAppearanceUpdate();
            });
        }

        private UIViewController GetCurrentViewController()
        {
            UIWindow window = UIApplication.SharedApplication.KeyWindow;
            UIViewController vc = window.RootViewController;
            while (vc.PresentedViewController != null)
            {
                vc = vc.PresentedViewController;
            }
            return vc;
        }
    }
}