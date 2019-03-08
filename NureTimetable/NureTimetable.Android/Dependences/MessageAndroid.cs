using Android.Widget;
using NureTimetable.Droid.Dependences;
using NureTimetable.Models.InterplatformCommunication;

[assembly: Xamarin.Forms.Dependency(typeof(MessageAndroid))]
namespace NureTimetable.Droid.Dependences
{
    public class MessageAndroid : IMessage
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