﻿using NureTimetable.Core.Localization;
using NureTimetable.Core.Models.Settings;
using NureTimetable.DAL;
using NureTimetable.UI.Helpers;
using NureTimetable.UI.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Essentials;
using static NureTimetable.UI.ViewModels.SettingsViewModel;
using AppTheme = NureTimetable.Core.Models.Settings.AppTheme;

namespace NureTimetable.UI.ViewModels
{
    public class MenuViewModel : BaseViewModel
    {
        #region Properties
        public LocalizedString AppVersion { get; } = new(() => string.Format(LN.Version, AppInfo.VersionString));

        public LocalizedString AppLanguageName { get; }

        public LocalizedString AppThemeName { get; }

        public IAsyncCommand OpenDonatePageCommand { get; }
        public IAsyncCommand ChangeThemeCommand { get; }
        public IAsyncCommand ChangeLanguageCommand { get; }
        public IAsyncCommand OpenSettingsCommand { get; }
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

            OpenDonatePageCommand = CommandFactory.Create(async () => await Navigation.PushAsync(new DonatePage()));
            ChangeThemeCommand = CommandFactory.Create(ChangeTheme);
            ChangeLanguageCommand = CommandFactory.Create(ChangeLanguage);
            OpenSettingsCommand = CommandFactory.Create(async () => await Navigation.PushAsync(new SettingsPage()));

            SettingsRepository.Settings.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(SettingsRepository.Settings.Theme))
                    OnPropertyChanged(nameof(AppThemeName));
            };
        }

        public Task ChangeTheme() =>
            ChangeSetting
            (
                LN.Theme,
                themeMapping,
                SettingsRepository.Settings.Theme,
                newTheme => SettingsRepository.Settings.Theme = newTheme
            );

        public Task ChangeLanguage() =>
            ChangeSetting
            (
                LN.Language,
                languageMapping,
                SettingsRepository.Settings.Language,
                newLanguage => SettingsRepository.Settings.Language = newLanguage
            );
    }
}