using NureTimetable.DAL;
using NureTimetable.Models;
using NureTimetable.Models.Consts;
using NureTimetable.Views.Lessons;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

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
            string action = await DisplayActionSheet("Выберете действие:", "Отмена", null, actionList.ToArray());
            switch (action)
            {
                case "Выбрать":
                    GroupsDataStore.UpdateSelected(selectedGroup);
                    await DisplayAlert("Выбор группы", "Группа успешно выбрана", "Ok");
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
                    if (selectedGroup.ID == GroupsDataStore.GetSelected()?.ID)
                    {
                        GroupsDataStore.UpdateSelected(null);
                    }
                    break;
            }
        }
        
        private bool CheckUpdateTimetableRights()
        {
            TimeSpan? timePass = DateTime.Now - SettingsDataStore.GetLastTimetableUpdate();
            if (timePass != null && timePass <= Config.TimetableManualUpdateMinInterval)
            {
                DisplayAlert("Обновление расписания", $"В связи с большой нагрузкой на cist, обновление расписания ограничено одним разом в 24 часа. Пожалуйста, подождите ещё {(Config.TimetableManualUpdateMinInterval - timePass.Value).ToString("hh\\:mm")}, и попробуйте снова.", "Хорошо");
                return false;
            }
            return true;
        }

        private async Task UpdateTimetable(params SavedGroup[] savedGroups)
        {
            if (savedGroups.Length == 0 || CheckUpdateTimetableRights() == false)
            {
                return;
            }

            GroupsLayout.IsEnabled = false;
            ProgressLayout.IsVisible = true;

            await Task.Factory.StartNew(() =>
            {
                string result;
                if (EventsDataStore.GetEventsFromCist(Config.TimetableFromDate, Config.TimetableToDate, savedGroups) != null)
                {
                    foreach (SavedGroup group in savedGroups)
                    {
                        group.LastUpdated = DateTime.Now;
                    }
                    GroupsDataStore.UpdateSaved(groups.ToList());

                    if (savedGroups.Length == 1)
                    {
                        result = $"Расписание группы {savedGroups[0].Name} успешно обновлено.";
                    }
                    else
                    {
                        result = $"Расписание успешно обновлено для групп:{Environment.NewLine}{string.Join(", ", savedGroups.Select(g => g.Name))}";
                    }
                }
                else
                {
                    result = "Произошла ошибка, пожалуйста, попробуйте позже.";
                }

                Device.BeginInvokeOnMainThread(() =>
                {
                    DisplayAlert("Обновление расписания", result, "Ok");
                    ProgressLayout.IsVisible = false;
                    GroupsLayout.IsEnabled = true;
                });
            });
        }

        private void AddGroup_Clicked(object sender, EventArgs e)
        {
            if (!GroupsLayout.IsEnabled)
            {
                return;
            }
            Navigation.PushAsync(new AddGroupPage());
        }

        private async void UpdateAll_Clicked(object sender, EventArgs e)
        {
            if (!GroupsLayout.IsEnabled)
            {
                return;
            }
            if (await DisplayAlert("Обновление расписания", "Обновить расписания всех сохранённых групп?", "Да", "Отмена"))
            {
                await UpdateTimetable(groups.ToArray());
            }
        }
    }
}
