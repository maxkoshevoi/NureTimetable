using NureTimetable.DAL;
using NureTimetable.Models;
using NureTimetable.Models.Consts;
using NureTimetable.Services.Helpers;
using NureTimetable.UI.Views.Groups;
using NureTimetable.ViewModels.Core;
using NureTimetable.Views.Lessons;
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
        #region Classes
        public class SavedGroupItemViewModel
        {
            #region variables
            private ManageGroupsViewModel _manageGroupsViewModel;
            #endregion

            #region Properties
            public SavedGroup SavedGroup { get; }

            // TODO: Move this property inside SavedGroup and replace SelectedGroup functionality with it
            public bool IsSelected { get; set; }

            public ICommand SettingsClickedCommand { get; }
            #endregion

            public SavedGroupItemViewModel(SavedGroup savedGroup, ManageGroupsViewModel manageGroupsViewModel)
            {
                SavedGroup = savedGroup;
                _manageGroupsViewModel = manageGroupsViewModel;
                SettingsClickedCommand = CommandHelper.CreateCommand(SettingsClicked);
            }
            
            public async Task SettingsClicked()
            {
                List<string> actionList = new List<string> { "Обновить расписание", "Настроить отображение предметов", "Удалить" };
                if (!IsSelected)
                {
                    actionList.Insert(0, "Выбрать");
                }
                if (Device.RuntimePlatform == Device.Android && CrossDeviceInfo.Current.VersionNumber.Major < 5)
                {
                    // SfCheckBox doesn`t support Android 4
                    actionList.Remove("Настроить отображение предметов");
                }
                string action = await App.Current.MainPage.DisplayActionSheet("Выберите действие:", "Отмена", null, actionList.ToArray());
                switch (action)
                {
                    case "Выбрать":
                        GroupsDataStore.UpdateSelected(SavedGroup);
                        await _manageGroupsViewModel.Navigation.PopToRootAsync();
                        break;
                    case "Обновить расписание":
                        await _manageGroupsViewModel.UpdateTimetable(SavedGroup);
                        break;
                    case "Настроить отображение предметов":
                        await _manageGroupsViewModel.Navigation.PushAsync(new ManageLessonsPage(SavedGroup));
                        break;
                    case "Удалить":
                        _manageGroupsViewModel.Groups.Remove(this);
                        GroupsDataStore.UpdateSaved(_manageGroupsViewModel.Groups.Select(vm => vm.SavedGroup).ToList());
                        break;
                }
            }
        }
        #endregion

        #region variables
        private bool _isNoSourceLayoutVisable;

        private bool _isGroupsLayoutEnable;

        private bool _isProgressVisable;

        private ObservableCollection<SavedGroupItemViewModel> _groups;
        #endregion

        #region Properties
        public bool IsNoSourceLayoutVisable
        {
            get => _isNoSourceLayoutVisable;
            set => SetProperty(ref _isNoSourceLayoutVisable, value);
        }

        public bool IsProgressVisable
        {
            get => _isProgressVisable;
            set => SetProperty(ref _isProgressVisable, value);
        }

        public bool IsGroupsLayoutEnalbe
        {
            get => _isGroupsLayoutEnable;
            set => SetProperty(ref _isGroupsLayoutEnable, value);
        }

        private SavedGroupItemViewModel _savedGroupSelectedItem;

        public SavedGroupItemViewModel SavedGroupSelectedItem
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

        public ObservableCollection<SavedGroupItemViewModel> Groups { get => _groups; set => SetProperty(ref _groups, value); }

        public ICommand UpdateAllCommand { get; }

        public ICommand AddGroupCommand { get; }

        #endregion

        public ManageGroupsViewModel(INavigation navigation) : base(navigation)
        {
            IsProgressVisable = false;
            IsGroupsLayoutEnalbe = true;

            UpdateItems(GroupsDataStore.GetSaved());
            MessagingCenter.Subscribe<Application, List<SavedGroup>>(this, MessageTypes.SavedGroupsChanged, (sender, newSavedGroups) => 
            {
                UpdateItems(newSavedGroups);
            });

            UpdateAllCommand = CommandHelper.CreateCommand(UpdateAll);
            AddGroupCommand = CommandHelper.CreateCommand(AddGroup);
        }

        #region Methods
        public async Task SavedGroupSelected(SavedGroupItemViewModel selectedGroup)
        {
            if (!selectedGroup.IsSelected)
            {
                GroupsDataStore.UpdateSelected(selectedGroup.SavedGroup);
            }
            await Navigation.PopToRootAsync();
        }

        private async Task UpdateAll()
        {
            if (!IsGroupsLayoutEnalbe)
            {
                return;
            }
            if (await App.Current.MainPage.DisplayAlert("Обновление расписания", "Обновить расписания всех сохранённых групп?", "Да", "Отмена"))
            {
                await UpdateTimetable(Groups?.Select(vm => vm.SavedGroup).ToArray());
            }
        }

        private async Task AddGroup()
        {
            if (!IsGroupsLayoutEnalbe)
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
            IsNoSourceLayoutVisable = (newItems.Count == 0);

            Group selectedGroup = GroupsDataStore.GetSelected();
            Groups = new ObservableCollection<SavedGroupItemViewModel>(
                newItems.Select(sg => new SavedGroupItemViewModel(sg, this)
                {
                    IsSelected = sg.ID == selectedGroup?.ID
                })
            );
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
                await App.Current.MainPage.DisplayAlert("Обновление расписания", "У вас уже загружена последняя версия расписания", "Ок");
                return;
            }

            IsGroupsLayoutEnalbe = false;
            IsProgressVisable = true;

            await Task.Factory.StartNew(() =>
            {
                string result;
                if (EventsDataStore.GetTimetableFromCist(Config.TimetableFromDate, Config.TimetableToDate, groupsAllowed.ToArray()) != null)
                {
                    if (groupsAllowed.Count == 1)
                    {
                        result = $"Расписание группы {groupsAllowed[0].Name} успешно обновлено.";
                    }
                    else
                    {
                        result = $"Расписание успешно обновлено для групп:{Environment.NewLine}{string.Join(", ", groupsAllowed.Select(g => g.Name))}";
                    }
                }
                else
                {
                    result = "Произошла ошибка, пожалуйста, попробуйте позже.";
                }

                Device.BeginInvokeOnMainThread(async () =>
                {
                    IsProgressVisable = false;
                    IsGroupsLayoutEnalbe = true;
                    if (await App.Current.MainPage.DisplayAlert("Обновление расписания", result, "К расписанию", "Ok"))
                    {
                        await Navigation.PopToRootAsync();
                    }
                });
            });
        }
        #endregion
    }
}