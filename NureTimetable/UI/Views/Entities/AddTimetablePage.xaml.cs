using NureTimetable.UI.ViewModels;

namespace NureTimetable.UI.Views;

public partial class AddTimetablePage : TabbedPage
{
    public AddTimetablePage()
    {
        InitializeComponent();
        BindingContext = new AddTimetableViewModel();
    }
}
