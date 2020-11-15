using Android.Widget;
using NureTimetable.Core.Models.InterplatformCommunication;
using NureTimetable.Droid.Dependences;

[assembly: Xamarin.Forms.Dependency(typeof(MessageManager))]
namespace NureTimetable.Droid.Dependences
{
    public class MessageManager : IMessageManager
    {
        public void LongAlert(string message)
        {
            Toast.MakeText(Android.App.Application.Context, message, ToastLength.Long).Show();
        }

        public void ShortAlert(string message)
        {
            Toast.MakeText(Android.App.Application.Context, message, ToastLength.Short).Show();
        }
    }
}