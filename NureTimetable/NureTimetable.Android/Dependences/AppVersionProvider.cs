using Xamarin.Forms;
using NureTimetable.Droid.Dependences;
using NureTimetable.Core.Models.InterplatformCommunication;

[assembly: Dependency(typeof(AppVersionProvider))]
namespace NureTimetable.Droid.Dependences
{
    public class AppVersionProvider : IAppVersionProvider
    {
        public string AppVersion
        {
            get
            {
                var context = Android.App.Application.Context;
                var info = context.PackageManager.GetPackageInfo(context.PackageName, 0);
                return info.VersionName;
            }
        }
    }
}