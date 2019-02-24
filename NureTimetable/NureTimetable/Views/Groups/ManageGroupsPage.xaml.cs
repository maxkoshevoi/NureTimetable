using NureTimetable.DAL;
using NureTimetable.Models;
using NureTimetable.Models.Consts;
using NureTimetable.Views.Lessons;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using NureTimetable.UI.Views.Groups;
using NureTimetable.ViewModels.Groups;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Plugin.DeviceInfo;

namespace NureTimetable.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ManageGroupsPage : ContentPage
    {
        public ObservableCollection<SavedGroup> groups { get; set; }

        public ManageGroupsPage()
        {
            InitializeComponent();

            UpdateItems(GroupsDataStore.GetSaved());
            MessagingCenter.Subscribe<Application, List<SavedGroup>>(this, MessageTypes.SavedGroupsChanged, (sender, newSavedGroups) =>
            {
                UpdateItems(newSavedGroups);
            });
        }

        private void UpdateItems(List<SavedGroup> newItems)
        {
            NoSourceLayout.IsVisible = (newItems.Count == 0);

            groups = new ObservableCollection<SavedGroup>(newItems);
            GroupsList.ItemsSource = groups;
        }

        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null || !(e.Item is SavedGroup))
                return;

            SavedGroup selectedGroup = (SavedGroup)e.Item;
            //Deselect Item
            ((ListView)sender).SelectedItem = null;

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
            string action = await DisplayActionSheet("Выберете действие:", "Отмена", null, actionList.ToArray());
            switch (action)
            {
                case "Выбрать":
                    GroupsDataStore.UpdateSelected(selectedGroup);
                    if (await DisplayAlert("Выбор группы", "Группа успешно выбрана", "К расписанию", "Ok"))
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
                    groups.Remove(selectedGroup);
                    GroupsDataStore.UpdateSaved(groups.ToList());
                    break;
            }
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
                await DisplayAlert("Обновление расписания", "У вас уже загружена последняя версия расписания", "Ок");
                return;
            }

            GroupsLayout.IsEnabled = false;
            ProgressLayout.IsVisible = true;

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
                    ProgressLayout.IsVisible = false;
                    GroupsLayout.IsEnabled = true;
                    if (await DisplayAlert("Обновление расписания", result, "К расписанию", "Ok"))
                    {
                        await Navigation.PopAsync();
                    }
                });
            });
        }

        private void AddGroup_Clicked(object sender, EventArgs e)
        {
            if (!GroupsLayout.IsEnabled)
            {
                return;
            }
            Navigation.PushAsync(new AddGroupPage()
            {
                BindingContext = new AddGroupViewModel(Navigation)
            });
        }

        private async void UpdateAll_Clicked(object sender, EventArgs e)
        {
            if (!GroupsLayout.IsEnabled)
            {
                return;
            }
            if (await DisplayAlert("Обновление расписания", "Обновить расписания всех сохранённых групп?", "Да", "Отмена"))
            {
                await UpdateTimetable(groups?.ToArray());
            }
        }
    }
}
