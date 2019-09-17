using NureTimetable.Core.Models.InterplatformCommunication;
using NureTimetable.Services.Helpers;
using NureTimetable.ViewModels.Core;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace NureTimetable.ViewModels.Info
{
    public class AboutViewModel : BaseViewModel
    {
        #region Properties
        public string VersionText { get; set; }

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

        public AboutViewModel(INavigation navigation) : base(navigation)
        {
            NavigateUriCommand = CommandHelper.CreateCommand<string>(NavigateUri);
            var version = DependencyService.Get<IAppVersionProvider>();
            VersionText = version?.AppVersion ?? "-";
        }

        private async Task NavigateUri(string url)
        {
            Device.OpenUri(new Uri(url));
        }
    }
}