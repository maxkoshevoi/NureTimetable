using NureTimetable.ViewModels.Core;
using Xamarin.Forms;

namespace NureTimetable.ViewModels.TimetableEntities
{
    public class AddTimetableViewModel : BaseViewModel
    {
        #region Properties
        public AddGroupViewModel AddGroupPageViewModel { get; }
        public AddGroupViewModel AddTeacherPageViewModel { get; }
        #endregion

        public AddTimetableViewModel(INavigation navigation) : base(navigation)
        {
            AddGroupPageViewModel = new AddGroupViewModel(Navigation);
            AddTeacherPageViewModel = new AddGroupViewModel(Navigation);
        }
    }
}
