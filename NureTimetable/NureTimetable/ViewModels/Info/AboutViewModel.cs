using NureTimetable.Core.Models.InterplatformCommunication;
using NureTimetable.Services.Helpers;
using NureTimetable.ViewModels.Core;
using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace NureTimetable.ViewModels.Info
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

        public AboutViewModel(INavigation navigation) : base(navigation)
        {
            var version = DependencyService.Get<IAppVersionProvider>();
            VersionText = version?.AppVersion ?? "-";

            NavigateUriCommand = CommandHelper.CreateCommand<string>(NavigateUri);
        }

        private void NavigateUri(string url)
        {
            Device.OpenUri(new Uri(url));
        }
    }
}