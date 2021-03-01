using NureTimetable.BL;
using NureTimetable.Core.Localization;
using NureTimetable.DAL;
using NureTimetable.UI.Helpers;
using Plugin.Calendars.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace NureTimetable.UI.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        #region Properties
        public LocalizedString DefaultCalendarName { get; }

        public IAsyncCommand PageAppearingCommand { get; }
        public Command ToggleDebugModeCommand { get; }
        public Command ToggleAutoupdateCommand { get; }
        public IAsyncCommand ChangeDefaultCalendarCommand { get; }
        #endregion

        #region Setting mappings
        List<(Func<string> name, string id)> calendarMapping;
        #endregion

        public SettingsViewModel()
        {
            DefaultCalendarName = new(() =>
            {
                if (calendarMapping == null)
                    return LN.Wait;

                return calendarMapping.SingleOrDefault(m => m.id == SettingsRepository.Settings.DefaultCalendarId).name?.Invoke() ?? LN.InsufficientRights;
            });

            PageAppearingCommand = CommandFactory.Create(PageAppearing);
            ChangeDefaultCalendarCommand = CommandFactory.Create(ChangeDefaultCalendar, allowsMultipleExecutions: false);
            ToggleDebugModeCommand = CommandFactory.Create(() => SettingsRepository.Settings.IsDebugMode = !SettingsRepository.Settings.IsDebugMode);
            ToggleAutoupdateCommand = CommandFactory.Create(() => SettingsRepository.Settings.Autoupdate = !SettingsRepository.Settings.Autoupdate);
        }

        private async Task PageAppearing()
        {
            if (calendarMapping == null)
            {
                await UpdateDefaultCalendarMapping(false);
            }
        }

        public static async Task ChangeSetting<T>(string name, List<(Func<string> name, T value)> mapping, T currectValue, Action<T> applyNewValue)
        {
            string selectedName = await Shell.Current.DisplayActionSheet(name, LN.Cancel, null, mapping.Select(m => m.name()).ToArray());
            if (selectedName == null || selectedName == LN.Cancel)
                return;

            T selectedValue = mapping.Single(m => m.name() == selectedName).value;
            if (currectValue?.Equals(selectedValue) == true)
            {
                return;
            }

            applyNewValue(selectedValue);
        }

        private async Task UpdateDefaultCalendarMapping(bool requestPermissionIfNeeded)
        {
            List<(Func<string>, string)> newMapping = new() 
            { 
                (() => LN.AskEveryTime, string.Empty) 
            };

            if (requestPermissionIfNeeded || await CalendarService.CheckPermissionsAsync())
            {
                IList<Calendar> calendars = await CalendarService.GetAllCalendarsAsync();
                newMapping.AddRange(calendars.Select(c => ((Func<string>)(() => c.Name), c.ExternalID)));
            }

            calendarMapping = newMapping;
            OnPropertyChanged(nameof(DefaultCalendarName));
        }

        public async Task ChangeDefaultCalendar()
        {
            await UpdateDefaultCalendarMapping(true);
            await ChangeSetting
            (
                LN.DefaultCalendar,
                calendarMapping,
                SettingsRepository.Settings.DefaultCalendarId,
                newCalendar => 
                {
                    SettingsRepository.Settings.DefaultCalendarId = newCalendar;
                    OnPropertyChanged(nameof(DefaultCalendarName));
                }
            );
        }
    }
}