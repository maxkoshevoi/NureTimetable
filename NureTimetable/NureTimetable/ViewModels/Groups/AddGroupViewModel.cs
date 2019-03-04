using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using NureTimetable.Core.Localization;
using NureTimetable.DAL;
using NureTimetable.Models;
using NureTimetable.Services.Helpers;
using NureTimetable.ViewModels.Core;
using Xamarin.Forms;

namespace NureTimetable.ViewModels.Groups
{
    public class AddGroupViewModel : BaseViewModel
    {
        #region variables

        private List<Group> _allGroups;

        private List<SavedGroup> _savedGroups;

        private ObservableCollection<Group> _groups;

        private bool _progressLayoutIsVisible;

        private bool _progressLayoutIsEnabled;

        private string _searchBarText;

        private Group _selectedGroup;

        #endregion

        #region Properties

        public ObservableCollection<Group> Groups { get => _groups; set => SetProperty(ref _groups, value); }

        public bool ProgressLayoutIsVisible
        {
            get => _progressLayoutIsVisible;
            set => SetProperty(ref _progressLayoutIsVisible, value);
        }

        public bool ProgressLayoutIsEnabled
        {
            get => _progressLayoutIsEnabled;
            set => SetProperty(ref _progressLayoutIsEnabled, value);
        }

        public string SearchBarText { get => _searchBarText; set => SetProperty(ref _searchBarText, value); }

        public ICommand UpdateCommand { get; protected set; }

        public ICommand SearchBarTextChangedCommand { get; protected set; }

        public ICommand ContentPageAppearingCommand { get; protected set; }

        #endregion

        public AddGroupViewModel(INavigation navigation) : base(navigation)
        {
            SearchBarTextChangedCommand = CommandHelper.CreateCommand(SearchBarTextChanged);
            ContentPageAppearingCommand = CommandHelper.CreateCommand(UpdateGroups);
            UpdateCommand = CommandHelper.CreateCommand(UpdateFromCist);
            Device.BeginInvokeOnMainThread((async () => await UpdateGroups(false)));
        }

        #region Methods

        public Group SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                if (value != null)
                    Device.BeginInvokeOnMainThread(async () => { await GroupSelected(value); });

                _selectedGroup = value;
            }
        }

        private async Task GroupSelected(Group group)
        {
            if (_savedGroups.Exists(g => g.ID == group.ID))
            {
                await App.Current.MainPage.DisplayAlert(LN.AddingGroup, LN.GroupAlreadySaved, LN.Ok);
                return;
            }

            _savedGroups.Add(new SavedGroup(group));

            GroupsDataStore.UpdateSaved(_savedGroups);

            await App.Current.MainPage.DisplayAlert(LN.AddingGroup, LN.GroupSaved, LN.Ok);
        }

        private async Task SearchBarTextChanged()
        {
            if (_allGroups == null) return;

            if (string.IsNullOrEmpty(SearchBarText))
            {
                Groups = new ObservableCollection<Group>(_allGroups.OrderBy(g => g.Name));
            }
            else
            {
                string searchQuery = SearchBarText.ToLower();
                Groups =
                    new ObservableCollection<Group>(
                        _allGroups
                            .Where(g =>
                                g.Name.ToLower().Contains(searchQuery) || 
                                g.Name.ToLower().Contains(searchQuery.Replace('и', 'і')) || 
                                g.ID.ToString() == searchQuery)
                            .OrderBy(g => g.Name)
                    );
            }
        }

        private async Task UpdateFromCist()
        {
            if (SettingsDataStore.CheckCistAllGroupsUpdateRights() == false)
            {
                await App.Current.MainPage.DisplayAlert(LN.GroupsListLoading, LN.GroupsListLatest, LN.Ok);
                return;
            }

            bool displayAlert = await App.Current.MainPage.DisplayAlert(
                LN.GroupsListLoading,
                LN.GroupsListLoadingConfirmation,
                LN.Yes, LN.Cancel
            );

            if (!ProgressLayoutIsVisible && displayAlert)
            {
                await UpdateGroups(true);
            }     
        }

        private async Task UpdateGroups()
        {
            await UpdateGroups(false);
        }

        private async Task UpdateGroups(bool fromCistOnly = false)
        {
            ProgressLayoutIsVisible = true;
            ProgressLayoutIsEnabled = false;

            await Task.Factory.StartNew(() =>
            {
                if (fromCistOnly)
                {
                    _allGroups = GroupsDataStore.GetAllFromCist();
                }
                else
                {
                    _allGroups = GroupsDataStore.GetAll();
                }

                if (_allGroups == null)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        App.Current.MainPage.DisplayAlert(
                            LN.GroupsListLoading,
                            LN.GroupsListLoadingFail,
                            LN.Ok
                        );
                    });

                    ProgressLayoutIsVisible = false;
                    ProgressLayoutIsEnabled = true;

                    return;
                }

                _savedGroups = GroupsDataStore.GetSaved();
                Groups = new ObservableCollection<Group>(_allGroups.OrderBy(g => g.Name));

                if (SearchBarTextChangedCommand.CanExecute(null))
                {
                    SearchBarTextChangedCommand.Execute(null);
                } 

                ProgressLayoutIsVisible = false;
                ProgressLayoutIsEnabled = true;
            });
        }
        #endregion
    }
}