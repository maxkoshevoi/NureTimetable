using NureTimetable.DAL;
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

        private void ContentPage_Appearing(object sender, EventArgs e)
        {
            GroupsLayout.IsEnabled = false;
            ProgressLayout.IsVisible = true;

            Task.Factory.StartNew(() =>
            {
                allGroups = GroupsDataStore.GetAll();
                if (allGroups == null)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        DisplayAlert("Загрузка списка групп", "Не удолось загрузить список групп. Пожалуйста, попробуйте позже.", "Ok");
                        Navigation.PopAsync();
                    });
                    return;
                }

                savedGroups = GroupsDataStore.GetSaved();
                groups = new ObservableCollection<Group>(allGroups.OrderBy(g => g.Name));

                Device.BeginInvokeOnMainThread(() =>
                {
                    AllGroupsList.ItemsSource = groups;

                    ProgressLayout.IsVisible = false;
                    GroupsLayout.IsEnabled = true;
                });
            });
        }

        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null && e.Item is Group)
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
            string searchQuery = Searchbar.Text.ToLower();
            groups =
                new ObservableCollection<Group>(
                    allGroups
                    .Where(g => g.Name.ToLower().Contains(searchQuery) || g.ID.ToString() == searchQuery)
                    .OrderBy(g => g.Name)
                );
            AllGroupsList.ItemsSource = groups;
        }
    }
}
