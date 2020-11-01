﻿using NureTimetable.UI.ViewModels.TimetableEntities.ManageEntities;
using Xamarin.Forms;

namespace NureTimetable.UI.Views
{
    public partial class ManageEntitiesPage : ContentPage
    {
        public ManageEntitiesPage()
        {
            InitializeComponent();
            BindingContext = new ManageEntitiesViewModel();
        }
        
        private void EntitiesList_OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            ((ListView)sender).SelectedItem = null;
        }
    }
}
