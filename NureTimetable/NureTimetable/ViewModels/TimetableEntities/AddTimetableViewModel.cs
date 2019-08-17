using NureTimetable.ViewModels.Core;
using Xamarin.Forms;

namespace NureTimetable.ViewModels.TimetableEntities
{
    public class AddTimetableViewModel : BaseViewModel
    {
        #region Properties
        public AddGroupViewModel AddGroupPageViewModel { get; }
        public AddTeacherViewModel AddTeacherPageViewModel { get; }
        public AddRoomViewModel AddRoomPageViewModel { get; }
        #endregion

        public AddTimetableViewModel(INavigation navigation) : base(navigation)
        {
            AddGroupPageViewModel = new AddGroupViewModel(Navigation);
            AddTeacherPageViewModel = new AddTeacherViewModel(Navigation);
            AddRoomPageViewModel = new AddRoomViewModel(Navigation);
        }
    }
}
