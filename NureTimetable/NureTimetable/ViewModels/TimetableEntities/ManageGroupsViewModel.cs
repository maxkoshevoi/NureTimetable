using NureTimetable.Core.Models.Consts;
using NureTimetable.DAL;
using NureTimetable.DAL.Models.Local;
using NureTimetable.Services.Helpers;
using NureTimetable.UI.Views.TimetableEntities;
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

namespace NureTimetable.ViewModels.TimetableEntities
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
            public SavedEntity SavedGroup { get; }

            // TODO: Move this property inside SavedGroup and replace SelectedGroup functionality with it
            public bool IsSelected { get; set; }

            public ICommand SettingsClickedCommand { get; }
            #endregion

            public SavedGroupItemViewModel(SavedEntity savedGroup, ManageGroupsViewModel manageGroupsViewModel)
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
                if (Device.RuntimePlatform == Device.Android 
                    && CrossDeviceInfo.Current.VersionNumber.Major > 0 
                    && CrossDeviceInfo.Current.VersionNumber.Major < 5)
                {
                    // SfCheckBox doesn`t support Android 4
                    actionList.Remove("Настроить отображение предметов");
                }
                string action = await App.Current.MainPage.DisplayActionSheet("Выберите действие:", "Отмена", null, actionList.ToArray());
                switch (action)
                {
                    case "Выбрать":
                        UniversityEntitiesRepository.UpdateSelected(SavedGroup);
                        await _manageGroupsViewModel.Navigation.PopToRootAsync();
                        break;
                    case "Обновить расписание":
                        await _manageGroupsViewModel.UpdateTimetable(SavedGroup);
                        break;
                    case "Настроить отображение предметов":
                        await _manageGroupsViewModel.Navigation.PushAsync(new ManageLessonsPage(new Group
                        {
                            ID = SavedGroup.ID,
                            Name = SavedGroup.Name
                        }));
                        break;
                    case "Удалить":
                        _manageGroupsViewModel.Groups.Remove(this);
                        UniversityEntitiesRepository.UpdateSaved(_manageGroupsViewModel.Groups.Select(vm => vm.SavedGroup).ToList());
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

            UpdateItems(UniversityEntitiesRepository.GetSaved());
            MessagingCenter.Subscribe<Application, List<SavedEntity>>(this, MessageTypes.SavedEntitiesChanged, (sender, newSavedGroups) => 
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
                UniversityEntitiesRepository.UpdateSelected(selectedGroup.SavedGroup);
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
            await Navigation.PushAsync(new AddTimetablePage()
            {
                BindingContext = new AddTimetableViewModel(Navigation)
            });
        }

        private void UpdateItems(List<SavedEntity> newItems)
        {
            IsNoSourceLayoutVisable = (newItems.Count == 0);

            SavedEntity selectedGroup = UniversityEntitiesRepository.GetSelected();
            Groups = new ObservableCollection<SavedGroupItemViewModel>(
                newItems.Select(sg => new SavedGroupItemViewModel(sg, this)
                {
                    IsSelected = sg.ID == selectedGroup?.ID
                })
            );
        }

        private async Task UpdateTimetable(params SavedEntity[] groups)
        {
            if (groups == null || groups.Length == 0)
            {
                return;
            }

            List<SavedEntity> groupsAllowed = SettingsRepository.CheckCistTimetableUpdateRights(groups);
            if (groupsAllowed.Count == 0)
            {
                await App.Current.MainPage.DisplayAlert("Обновление расписания", "У вас уже загружена последняя версия расписания", "Ок");
                return;
            }

            IsGroupsLayoutEnalbe = false;
            IsProgressVisable = true;

            await Task.Factory.StartNew(() =>
            {
                List<string> success = new List<string>(), fail = new List<string>();
                foreach (SavedEntity group in groupsAllowed)
                {
                    if (EventsRepository.GetTimetableFromCist(group, Config.TimetableFromDate, Config.TimetableToDate) != null)
                    {
                        success.Add(group.Name);
                    }
                    else
                    {
                        fail.Add(group.Name);
                    }
                }
                string result = "";
                if (success.Count > 0)
                {
                    result += $"Расписание успешно обновлено: {string.Join(", ", success)}{Environment.NewLine}";
                }
                if (fail.Count > 0)
                {
                    result += $"Произошла ошибка: {string.Join(", ", fail)}";
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