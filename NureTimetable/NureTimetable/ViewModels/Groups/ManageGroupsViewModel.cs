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
        #region variables
        private bool _isNoSourceLayoutVisable;

        private bool _isGroupsLayoutEnable;

        private bool _isProgressVisable;

        private ObservableCollection<SavedGroup> _groups;
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
            IsProgressVisable = false;
            IsGroupsLayoutEnalbe = true;
            UpdateItems(GroupsDataStore.GetSaved());
            MessagingCenter.Subscribe<Application, List<SavedGroup>>(this, MessageTypes.SavedGroupsChanged,
                (sender, newSavedGroups) => { UpdateItems(newSavedGroups); });
            UpdateAllCommand = CommandHelper.CreateCommand(UpdateAll);
            AddGroupCommand = CommandHelper.CreateCommand(AddGroup);
        }

        #region Methods
        public async Task SavedGroupSelected(SavedGroup selectedGroup)
        {
            List<string> actionList = new List<string> { "Обновить расписание", "Настроить отображение предметов", "Удалить" };
            if (GroupsDataStore.GetSelected()?.ID != selectedGroup.ID)
            {
                actionList.Insert(0, "Выбрать");
            }
            if (Device.RuntimePlatform == Device.Android && CrossDeviceInfo.Current.VersionNumber.Major < 5)
            {
                // It seems like SfCheckBox doesn`t support Android 4
                actionList.Remove("Настроить отображение предметов");
            }
            string action = await App.Current.MainPage.DisplayActionSheet("Выберете действие:", "Отмена", null, actionList.ToArray());
            switch (action)
            {
                case "Выбрать":
                    GroupsDataStore.UpdateSelected(selectedGroup);
                    if (await App.Current.MainPage.DisplayAlert("Выбор группы", "Группа успешно выбрана", "К расписанию", "Ok"))
                    {
                        await Navigation.PopAsync();
                    }
                    break;
                case "Обновить расписание":
                    await UpdateTimetable(selectedGroup);
                    break;
                case "Настроить отображение предметов":
                    await Navigation.PushAsync(new ManageLessonsPage(selectedGroup));
                    break;
                case "Удалить":
                    Groups.Remove(selectedGroup);
                    GroupsDataStore.UpdateSaved(Groups.ToList());
                    break;
            }
        }

        private async Task UpdateAll()
        {
            if (!IsGroupsLayoutEnalbe)
            {
                return;
            }
            if (await App.Current.MainPage.DisplayAlert("Обновление расписания", "Обновить расписания всех сохранённых групп?", "Да", "Отмена"))
            {
                await UpdateTimetable(Groups?.ToArray());
            }
        }

        private async Task AddGroup()
        {
            if (!IsGroupsLayoutEnalbe)
            {
                return;
            }
            Navigation.PushAsync(new AddGroupPage()
            {
                BindingContext = new AddGroupViewModel(Navigation)
            });
        }

        private void UpdateItems(List<SavedGroup> newItems)
        {
            IsNoSourceLayoutVisable = (newItems.Count == 0);

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