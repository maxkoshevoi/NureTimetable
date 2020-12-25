using Foundation;
using NureTimetable.Core.Models.InterplatformCommunication;
using NureTimetable.iOS.Dependences;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(MessageManager))]
namespace NureTimetable.iOS.Dependences
{
    public class MessageManager : IMessageManager
    {
        private const double LONG_DELAY = 3.5;
        private const double SHORT_DELAY = 2.0;

        private NSTimer alertDelay;
        private UIAlertController alert;

        public void LongAlert(string message)
        {
            ShowAlert(message, LONG_DELAY);
        }
        public void ShortAlert(string message)
        {
            ShowAlert(message, SHORT_DELAY);
        }

        private void ShowAlert(string message, double seconds)
        {
            alertDelay = NSTimer.CreateScheduledTimer(seconds, _ => DismissMessage());
            alert = UIAlertController.Create(null, message, UIAlertControllerStyle.Alert);
            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(alert, true, null);
        }

        private void DismissMessage()
        {
            alert?.DismissViewController(true, null);
            alertDelay?.Dispose();
        }
    }
}