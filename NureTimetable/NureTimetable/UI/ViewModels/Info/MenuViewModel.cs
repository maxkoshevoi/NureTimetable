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
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Essentials;
using Xamarin.Forms;
using AppTheme = NureTimetable.Core.Models.Settings.AppTheme;

namespace NureTimetable.UI.ViewModels.Info
{
    public class MenuViewModel : BaseViewModel
    {
        #region Properties
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

        public IAsyncCommand<string> NavigateUriCommand { get; }
        public Command ToggleDebugModeCommand { get; }
        public IAsyncCommand OpenDonatePageCommand { get; }
        public IAsyncCommand ChangeThemeCommand { get; }
        public IAsyncCommand ChangeLanguageCommand { get; }
        #endregion

        public MenuViewModel()
        {
            NavigateUriCommand = CommandHelper.Create<string>(async url => await Launcher.OpenAsync(new Uri(url)));
            ToggleDebugModeCommand = CommandHelper.Create(() => IsDebugModeActive = !IsDebugModeActive);
            OpenDonatePageCommand = CommandHelper.Create(async () => await Navigation.PushAsync(new DonatePage()));
            ChangeThemeCommand = CommandHelper.Create(ChangeTheme);
            ChangeLanguageCommand = CommandHelper.Create(ChangeLanguage);

            UpdateAppThemeName();
            MessagingCenter.Subscribe<Application, AppTheme>(Application.Current, MessageTypes.ThemeChanged, (sender, theme) => UpdateAppThemeName());
            UpdateAppLanguageName();
        }

        private void UpdateAppThemeName()
        {
            AppThemeName = SettingsRepository.Settings.Theme switch
            {
                AppTheme.Light => LN.LightTheme,
                AppTheme.Dark => LN.DarkTheme,
                AppTheme.FollowSystem => LN.FollowSystem,
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

            AppTheme theme = AppTheme.FollowSystem;
            if (themeStr == LN.LightTheme)
            {
                theme = AppTheme.Light;
            }
            else if (themeStr == LN.DarkTheme)
            {
                theme = AppTheme.Dark;
            }

            if (SettingsRepository.Settings.Theme == theme)
            {
                return;
            }
            SettingsRepository.Settings.Theme = theme;
            UpdateAppThemeName();

            ThemeHelper.SetAppTheme(theme);
        }

        public async Task ChangeLanguage()
        {
            string languageStr = await Shell.Current.DisplayActionSheet(LN.Language, LN.Cancel, null, LN.FollowSystem, LN.EnglishLanguage, LN.RussianLanguage, LN.UkrainianLanguage);
            if (languageStr is null)
                return;

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
            
            var activityManager = DependencyService.Get<IActivityManager>();
            activityManager.Recreate();
        }
    }
}