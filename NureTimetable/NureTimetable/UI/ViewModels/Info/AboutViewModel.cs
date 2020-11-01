using NureTimetable.UI.Helpers;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;

namespace NureTimetable.UI.ViewModels.Info
{
    public class AboutViewModel : BaseViewModel
    {
        #region Properties
        public string VersionText { get; }
        
        public bool IsDebugModeActive
        {
            get => App.IsDebugMode;
            set
            {
                App.IsDebugMode = value;
            }
        }

        public ICommand NavigateUriCommand { get; }
        #endregion

        public AboutViewModel()
        {
            VersionText = AppInfo.VersionString;
            NavigateUriCommand = CommandHelper.CreateCommand<string>(NavigateUri);
        }

        private async Task NavigateUri(string url)
        {
            await Launcher.OpenAsync(new Uri(url));
        }
    }
}