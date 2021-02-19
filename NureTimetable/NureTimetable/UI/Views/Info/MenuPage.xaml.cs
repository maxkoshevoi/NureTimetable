﻿using NureTimetable.UI.ViewModels;
using Xamarin.Forms;

namespace NureTimetable.UI.Views
{
    public partial class MenuPage : ContentPage
    {
        public MenuPage()
        {
            InitializeComponent();
            BindingContext = new MenuViewModel();
        }
    }
}