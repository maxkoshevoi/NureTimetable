using Microsoft.Maui.Controls;
using NureTimetable.UI.ViewModels;

namespace NureTimetable.UI.Views;

public partial class DlNureLogin : ContentPage
{
    public DlNureLogin()
    {
        InitializeComponent();
        BindingContext = new DlNureLoginViewModel();
    }
}
