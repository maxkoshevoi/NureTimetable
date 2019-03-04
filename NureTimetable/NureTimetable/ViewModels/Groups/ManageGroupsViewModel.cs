using NureTimetable.DAL;
using NureTimetable.Models;
using NureTimetable.Models.Consts;
using NureTimetable.Services.Helpers;
using NureTimetable.UI.Views.Groups;
using NureTimetable.ViewModels.Core;
using NureTimetable.Views.Lessons;
using NureTimetable.Core.Localization;
using Plugin.DeviceInfo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace NureTimetable.ViewModels.Groups
{
    public class ManageGroupsViewModel : BaseViewModel
    {
        #region variables
        private bool _isNoSourceLayoutVisible;

        private bool _isGroupsLayoutEnabled;

        private bool _isProgressVisible;

        private ObservableCollection<SavedGroup> _groups;
        #endregion

        #region Properties
        public bool IsNoSourceLayoutVisible
        {
            get => _isNoSourceLayoutVisible;
            set => SetProperty(ref _isNoSourceLayoutVisible, value);
        }

        public bool IsProgressVisible
        {
            get => _isProgressVisible;
            set => SetProperty(ref _isProgressVisible, value);
        }

        public bool IsGroupsLayoutEnabled
        {
            get => _isGroupsLayoutEnabled;
            set => SetProperty(ref _isGroupsLayoutEnabled, value);
        }

        private SavedGroup _savedGroupSelectedItem;

        public SavedGroup SavedGroupSelectedItem
        {
            get => _savedGroupSelectedItem;
            set
            {
                if (value != null)
                {
                    Device.BeginInvokeOnMainThread(async () => { await SavedGroupSelected(value); });
                }

                _savedGroupSelectedItem = value;
            }
        }

        public ObservableCollection<SavedGroup> Groups { get => _groups; set => SetProperty(ref _groups, value); }

        public ICommand UpdateAllCommand { get; protected set; }

        public ICommand AddGroupCommand { get; protected set; }

        #endregion

        public ManageGroupsViewModel(INavigation navigation) : base(navigation)
        {
            IsProgressVisible = false;
            IsGroupsLayoutEnabled = true;
            UpdateItems(GroupsDataStore.GetSaved());
            MessagingCenter.Subscribe<Application, List<SavedGroup>>(this, MessageTypes.SavedGroupsChanged,
                (sender, newSavedGroups) => { UpdateItems(newSavedGroups); });
            UpdateAllCommand = CommandHelper.CreateCommand(UpdateAll);
            AddGroupCommand = CommandHelper.CreateCommand(AddGroup);
        }

        #region Methods
        public async Task SavedGroupSelected(SavedGroup selectedGroup)
        {
            List<string> actionList = new List<string> { LN.UpdateTimetable, LN.SetUpLessonDisplay, LN.Delete };
            if (GroupsDataStore.GetSelected()?.ID != selectedGroup.ID)
            {
                actionList.Insert(0, LN.Select);
            }
            if (Device.RuntimePlatform == Device.Android && CrossDeviceInfo.Current.VersionNumber.Major < 5)
            {
                // It seems like SfCheckBox doesn`t support Android 4
                actionList.Remove(LN.SetUpLessonDisplay);
            }
            string action = await App.Current.MainPage.DisplayActionSheet(LN.ChooseAction, LN.Cancel, null, actionList.ToArray());

            if (action == LN.Select)
            {
                GroupsDataStore.UpdateSelected(selectedGroup);
                if (await App.Current.MainPage.DisplayAlert(LN.GroupSelect, LN.GroupSelected, LN.ToTimetable, LN.Ok))
                {
                    await Navigation.PopAsync();
                }
            }
            else if (action == LN.UpdateTimetable)
            {
                await UpdateTimetable(selectedGroup);
            }
            else if (action == LN.SetUpLessonDisplay)
            {
                await Navigation.PushAsync(new ManageLessonsPage(selectedGroup));
            }
            else if (action == LN.Delete)
            {
                Groups.Remove(selectedGroup);
                GroupsDataStore.UpdateSaved(Groups.ToList());
            }
        }

        private async Task UpdateAll()
        {
            if (!IsGroupsLayoutEnabled)
            {
                return;
            }
            if (await App.Current.MainPage.DisplayAlert(LN.TimetableUpdate, LN.UpdateSavedGroupsTimetable, LN.Yes, LN.Cancel))
            {
                await UpdateTimetable(Groups?.ToArray());
            }
        }

        private async Task AddGroup()
        {
            if (!IsGroupsLayoutEnabled)
            {
                return;
            }
            await Navigation.PushAsync(new AddGroupPage()
            {
                BindingContext = new AddGroupViewModel(Navigation)
            });
        }

        private void UpdateItems(List<SavedGroup> newItems)
        {
            IsNoSourceLayoutVisible = (newItems.Count == 0);

            Groups = new ObservableCollection<SavedGroup>(newItems);
        }

        private async Task UpdateTimetable(params Group[] groups)
        {
            if (groups == null || groups.Length == 0)
            {
                return;
            }

            List<SavedGroup> groupsAllowed = SettingsDataStore.CheckCistTimetableUpdateRights(groups);
            if (groupsAllowed.Count == 0)
            {
                await App.Current.MainPage.DisplayAlert(LN.TimetableUpdate, LN.TimetableLatest, LN.Ok);
                return;
            }

            IsGroupsLayoutEnabled = false;
            IsProgressVisible = true;

            await Task.Factory.StartNew(() =>
            {
                string result;
                if (EventsDataStore.GetTimetableFromCist(Config.TimetableFromDate, Config.TimetableToDate, groupsAllowed.ToArray()) != null)
                {
                    if (groupsAllowed.Count == 1)
                    {
                        result = string.Format(LN.GroupTimetableUpdated, groupsAllowed[0].Name);
                    }
                    else
                    {
                        var groupsUpdated = Environment.NewLine + string.Join(", ", groupsAllowed.Select(g => g.Name));
                        result = string.Format(LN.GroupsTimetableUpdated, groupsUpdated);
                    }
                }
                else
                {
                    result = LN.ErrorTryAgainLater;
                }

                Device.BeginInvokeOnMainThread(async () =>
                {
                    IsProgressVisible = false;
                    IsGroupsLayoutEnabled = true;
                    if (await App.Current.MainPage.DisplayAlert(LN.TimetableUpdate, result, LN.ToTimetable, LN.Ok))
                    {
                        try
                        {
                            await Navigation.PopAsync();
                        }
                        catch
                        {
                            // Sometimes this gives ArgumentOutOfRangeException
                        }
                    }
                });
            });
        }
        #endregion
    }
}