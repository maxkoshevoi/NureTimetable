using NureTimetable.UI.ViewModels.TimetableEntities;
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