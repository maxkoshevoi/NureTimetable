using System;
using NureTimetable.DAL;
using NureTimetable.UI.ViewModels.Core;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace NureTimetable.UI.ViewModels.TimetableEntities
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
            TimeSpan? timePass = DateTime.Now - SettingsRepository.GetLastCistAllEntitiesUpdateTime();
            bool isNeedReloadFromCist = !UniversityEntitiesRepository.IsInitialized && timePass > TimeSpan.FromDays(25);

            AddGroupPageViewModel = new AddGroupViewModel(Navigation);
            AddTeacherPageViewModel = new AddTeacherViewModel(Navigation);
            AddRoomPageViewModel = new AddRoomViewModel(Navigation);

            if (isNeedReloadFromCist)
            {
                MainThread.BeginInvokeOnMainThread(async () => await AddGroupPageViewModel.UpdateEntities(true));
            }
        }
    }
}
