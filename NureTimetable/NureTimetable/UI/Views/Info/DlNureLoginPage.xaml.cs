using NureTimetable.UI.ViewModels;
using Xamarin.Forms;

namespace NureTimetable.UI.Views.Info;

public partial class DlNureLogin : ContentPage
{
    public DlNureLogin()
    {
        InitializeComponent();
        BindingContext = new DlNureLoginViewModel();
    }
}
