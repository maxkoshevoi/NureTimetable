using NureTimetable.UI.ViewModels;
using Xamarin.Forms;

namespace NureTimetable.UI.Views
{
    public partial class AddTimetablePage : TabbedPage
    {
        public AddTimetablePage()
        {
            InitializeComponent();
            BindingContext = new AddTimetableViewModel();
        }
    }
}