using NureTimetable.DataStores;
using NureTimetable.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NureTimetable.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddGroupPage : ContentPage
    {
        List<Group> allGroups;
        List<SavedGroup> savedGroups;
        public ObservableCollection<Group> groups { get; set; }

        public AddGroupPage()
        {
            InitializeComponent();
        }

        private async void ContentPage_Appearing(object sender, EventArgs e)
        {
            await UpdateGroups();
        }

        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null || !(e.Item is Group))
                return;

            Group selectedGroup = (Group)e.Item;
            //Deselect Item
            ((ListView)sender).SelectedItem = null;

            if (savedGroups.Exists(g => g.ID == selectedGroup.ID))
            {
                await DisplayAlert("Добавление группы", "Группа уже находится в сохранённых", "OK");
                return;
            }

            savedGroups.Add(new SavedGroup(selectedGroup));
            GroupsDataStore.UpdateSaved(savedGroups);
            await DisplayAlert("Добавление группы", "Группа добавлена в сохранённые", "OK");
        }
        
        private void Searchbar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(Searchbar.Text))
            {
                groups = new ObservableCollection<Group>(allGroups.OrderBy(g => g.Name));
            }
            else
            {
                string searchQuery = Searchbar.Text.ToLower();
                groups =
                    new ObservableCollection<Group>(
                        allGroups
                        .Where(g => g.Name.ToLower().Contains(searchQuery) || g.Name.ToLower().Contains(searchQuery.Replace('и', 'і')) || g.ID.ToString() == searchQuery)
                        .OrderBy(g => g.Name)
                    );
            }
            AllGroupsList.ItemsSource = groups;
        }

        private async void UpdateFromCist_Clicked(object sender, EventArgs e)
        {
            if (ProgressLayout.IsVisible || await DisplayAlert("Обновление из Cist", "Вы уверенны, что хотите загрузить список групп из Cist?", "Да", "Отмена") == false)
            {
                return;
            }

            await UpdateGroups(true);
        }

        private async Task UpdateGroups(bool fromCistOnly = false)
        {
            GroupsLayout.IsEnabled = false;
            ProgressLayout.IsVisible = true;

            await Task.Factory.StartNew(() =>
            {
                if (fromCistOnly)
                {
                    allGroups = GroupsDataStore.GetAllFromCist();
                }
                else
                {
                    allGroups = GroupsDataStore.GetAll();
                }
                if (allGroups == null)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        DisplayAlert("Загрузка списка групп", "Не удолось загрузить список групп. Пожалуйста, попробуйте позже.", "Ok");
                    });
                    return;
                }

                savedGroups = GroupsDataStore.GetSaved();
                groups = new ObservableCollection<Group>(allGroups.OrderBy(g => g.Name));

                Device.BeginInvokeOnMainThread(() =>
                {
                    AllGroupsList.ItemsSource = groups;
                    Searchbar_TextChanged(new object(), new TextChangedEventArgs(null, null));

                    ProgressLayout.IsVisible = false;
                    GroupsLayout.IsEnabled = true;
                });
            });
        }
    }
}
