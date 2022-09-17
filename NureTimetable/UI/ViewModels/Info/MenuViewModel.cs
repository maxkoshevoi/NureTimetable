using static NureTimetable.UI.ViewModels.SettingsViewModel;

namespace NureTimetable.UI.ViewModels;

public class MenuViewModel : BaseViewModel
{
    #region Properties
    public LocalizedString AppVersion { get; } = new(() => string.Format(LN.Version, AppInfo.VersionString));

    public LocalizedString AppLanguageName { get; }

    public LocalizedString AppThemeName { get; }

    public IRelayCommand OpenDonatePageCommand { get; }
    public IRelayCommand ChangeThemeCommand { get; }
    public IRelayCommand ChangeLanguageCommand { get; }
    public IRelayCommand OpenSettingsCommand { get; }
    #endregion

    #region Setting mappings
    List<(Func<string> name, AppLanguage value)> languageMapping { get; } = new()
    {
        (() => LN.FollowSystem, AppLanguage.FollowSystem),
        (() => LN.EnglishLanguage, AppLanguage.English),
        (() => LN.RussianLanguage, AppLanguage.Russian),
        (() => LN.UkrainianLanguage, AppLanguage.Ukrainian),
    };

    List<(Func<string> name, AppTheme value)> themeMapping { get; } = new()
    {
        (() => LN.FollowSystem, AppTheme.FollowSystem),
        (() => LN.LightTheme, AppTheme.Light),
        (() => LN.DarkTheme, AppTheme.Dark),
    };
    #endregion

    public MenuViewModel()
    {
        AppLanguageName = new(() => languageMapping.Single(m => m.value == SettingsRepository.Settings.Language).name());
        AppThemeName = new(() => themeMapping.Single(m => m.value == SettingsRepository.Settings.Theme).name());

        OpenDonatePageCommand = CommandFactory.Create(() => Navigation.PushAsync(new DonatePage()));
        ChangeThemeCommand = CommandFactory.Create(ChangeTheme);
        ChangeLanguageCommand = CommandFactory.Create(ChangeLanguage);
        OpenSettingsCommand = CommandFactory.Create(() => Navigation.PushAsync(new SettingsPage()));

        SettingsRepository.Settings.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SettingsRepository.Settings.Theme))
                OnPropertyChanged(nameof(AppThemeName));
        };
    }

    public Task ChangeTheme() => ChangeSetting
    (
        LN.Theme,
        themeMapping,
        SettingsRepository.Settings.Theme,
        newTheme => SettingsRepository.Settings.Theme = newTheme
    );

    public Task ChangeLanguage() => ChangeSetting
    (
        LN.Language,
        languageMapping,
        SettingsRepository.Settings.Language,
        newLanguage => SettingsRepository.Settings.Language = newLanguage
    );
}
