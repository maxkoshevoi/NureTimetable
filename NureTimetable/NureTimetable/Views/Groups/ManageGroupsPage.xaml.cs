using NureTimetable.DAL;
using NureTimetable.Models;
using NureTimetable.Models.Consts;
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
            if (e.Item == null && e.Item is SavedGroup)
                return;

            SavedGroup selectedGroup = (SavedGroup)e.Item;
            //Deselect Item
            ((ListView)sender).SelectedItem = null;

            string action = await DisplayActionSheet("Выберете действие:", "Отмена", null, "Выбрать", "Обновить расписание", "Удалить");
            switch (action)
            {
                case "Выбрать":
                    GroupsDataStore.UpdateSelected(selectedGroup);
                    await DisplayAlert("Выбор группы", "Группа успешно выбрана", "Ok");
                    break;
                case "Обновить расписание":
                    GroupsLayout.IsEnabled = false;
                    ProgressLayout.IsVisible = true;

                    await Task.Factory.StartNew(() =>
                    {
                        string result;
                        if (TimetableDataStore.GetEventsFromCist(selectedGroup.ID, Config.TimetableFromDate, Config.TimetableToDate) != null)
                        {
                            selectedGroup.LastUpdated = DateTime.Now;
                            GroupsDataStore.UpdateSaved(groups.ToList());

                            result = $"Расписание группы {selectedGroup.Name} успешно обновлено.";
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

        private void AddGroup_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new AddGroupPage());
        }
    }
}
