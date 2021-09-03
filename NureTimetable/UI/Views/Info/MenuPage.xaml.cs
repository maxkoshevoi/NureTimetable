using Microsoft.Maui.Controls;
using NureTimetable.UI.ViewModels;

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