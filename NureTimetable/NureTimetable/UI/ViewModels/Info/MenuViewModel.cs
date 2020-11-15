using NureTimetable.Core.Localization;
using NureTimetable.Core.Models.Consts;
using NureTimetable.Core.Models.InterplatformCommunication;
using NureTimetable.Core.Models.Settings;
using NureTimetable.DAL;
using NureTimetable.UI.Helpers;
using NureTimetable.UI.Themes;
using NureTimetable.UI.Views;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using Settings = NureTimetable.Core.Models.Settings;

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

        private bool langIsRestartRequired = false;
        private string appLanguageName;
        public string AppLanguageName { get => appLanguageName; set => SetProperty(ref appLanguageName, value); }

        private string appThemeName;
        public string AppThemeName { get => appThemeName; set => SetProperty(ref appThemeName, value); }

        public ICommand NavigateUriCommand { get; }
        public ICommand ToggleDebugModeCommand { get; }
        public ICommand OpenAboutPageCommand { get; }
        public ICommand OpenDonatePageCommand { get; }
        public ICommand ChangeThemeCommand { get; }
        public ICommand ChangeLanguageCommand { get; }
        #endregion

        public MenuViewModel()
        {
            VersionText = AppInfo.VersionString;
            NavigateUriCommand = CommandHelper.Create<string>(async url => await Launcher.OpenAsync(new Uri(url)));
            ToggleDebugModeCommand = CommandHelper.Create(() => IsDebugModeActive = !IsDebugModeActive);
            OpenDonatePageCommand = CommandHelper.Create(async () => await Navigation.PushAsync(new DonatePage()));
            ChangeThemeCommand = CommandHelper.Create(ChangeTheme);
            ChangeLanguageCommand = CommandHelper.Create(ChangeLanguage);

            UpdateAppThemeName();
            MessagingCenter.Subscribe<Application, Settings.AppTheme>(Application.Current, MessageTypes.ThemeChanged, (sender, theme) =>
            {
                UpdateAppThemeName();
            });
            UpdateAppLanguageName();
        }

        private void UpdateAppThemeName()
        {
            AppThemeName = SettingsRepository.Settings.Theme switch
            {
                Settings.AppTheme.Light => LN.LightTheme,
                Settings.AppTheme.Dark => LN.DarkTheme,
                Settings.AppTheme.FollowSystem => LN.FollowSystem,
                _ => throw new InvalidOperationException("Unsuported theme")
            };
        }

        private void UpdateAppLanguageName()
        {
            string restartReqiredText = langIsRestartRequired ? $" ({LN.RestartRequired})" : string.Empty;

            AppLanguageName = SettingsRepository.Settings.Language switch
            {
                AppLanguage.English => LN.EnglishLanguage + restartReqiredText,
                AppLanguage.Russian => LN.RussianLanguage + restartReqiredText,
                AppLanguage.Ukrainian => LN.UkrainianLanguage + restartReqiredText,
                AppLanguage.FollowSystem => LN.FollowSystem + restartReqiredText,
                _ => throw new InvalidOperationException("Unsuported language")
            };
        }

        public async Task ChangeTheme()
        {
            string themeStr = await Shell.Current.DisplayActionSheet(LN.Theme, LN.Cancel, null, LN.FollowSystem, LN.LightTheme, LN.DarkTheme);
            if (themeStr is null)
            {
                return;
            }

            Settings.AppTheme theme = Settings.AppTheme.FollowSystem;
            if (themeStr == LN.LightTheme)
            {
                theme = Settings.AppTheme.Light;
            }
            else if (themeStr == LN.DarkTheme)
            {
                theme = Settings.AppTheme.Dark;
            }

            if (SettingsRepository.Settings.Theme == theme)
            {
                return;
            }
            SettingsRepository.Settings.Theme = theme;

            ThemeHelper.SetAppTheme(theme);
        }

        public async Task ChangeLanguage()
        {
            string languageStr = await Shell.Current.DisplayActionSheet(LN.Language, LN.Cancel, null, LN.FollowSystem, LN.EnglishLanguage, LN.RussianLanguage, LN.UkrainianLanguage);
            if (languageStr is null)
            {
                return;
            }

            AppLanguage language = AppLanguage.FollowSystem;
            if (languageStr == LN.EnglishLanguage)
            {
                language = AppLanguage.English;
            }
            else if (languageStr == LN.RussianLanguage)
            {
                language = AppLanguage.Russian;
            }
            else if (languageStr == LN.UkrainianLanguage)
            {
                language = AppLanguage.Ukrainian;
            }

            if (SettingsRepository.Settings.Language == language)
            {
                return;
            }
            SettingsRepository.Settings.Language = language;

            langIsRestartRequired = true;
            UpdateAppLanguageName();
        }
    }
}