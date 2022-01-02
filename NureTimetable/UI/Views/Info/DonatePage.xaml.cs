using Microsoft.Maui.Controls;
using NureTimetable.UI.ViewModels;

namespace NureTimetable.UI.Views;

public partial class DonatePage : ContentPage
{
    public DonatePage()
    {
        InitializeComponent();
        BindingContext = new DonateViewModel();
    }
}
