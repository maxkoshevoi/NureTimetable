using NureTimetable.UI.Helpers;
using NureTimetable.UI.Views;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;

namespace NureTimetable.UI.ViewModels.Info
{
    public class MenuViewModel : BaseViewModel
    {
        #region Properties
        public string VersionText { get; }
        
        public bool IsDebugModeActive
        {
            get => App.IsDebugMode;
            set
            {
                App.IsDebugMode = value;
                OnPropertyChanged();
            }
        }

        public ICommand NavigateUriCommand { get; }
        public ICommand ToggleDebugModeCommand { get; }
        public ICommand OpenAboutPageCommand { get; }
        public ICommand OpenDonatePageCommand { get; }
        #endregion

        public MenuViewModel()
        {
            VersionText = AppInfo.VersionString;
            NavigateUriCommand = CommandHelper.Create<string>(async url => await Launcher.OpenAsync(new Uri(url)));
            ToggleDebugModeCommand = CommandHelper.Create(() => IsDebugModeActive = !IsDebugModeActive);
            OpenDonatePageCommand = CommandHelper.Create(async () => await Navigation.PushAsync(new DonatePage()));
        }
    }
}