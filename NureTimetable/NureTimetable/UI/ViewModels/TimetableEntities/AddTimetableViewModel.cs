using NureTimetable.DAL;
using System;
using Xamarin.Essentials;

namespace NureTimetable.UI.ViewModels.TimetableEntities
{
    public class AddTimetableViewModel : BaseViewModel
    {
        #region Properties
        public AddGroupViewModel AddGroupPageViewModel { get; }
        public AddTeacherViewModel AddTeacherPageViewModel { get; }
        public AddRoomViewModel AddRoomPageViewModel { get; }
        #endregion

        public AddTimetableViewModel()
        {
            TimeSpan? timePass = DateTime.Now - SettingsRepository.GetLastCistAllEntitiesUpdateTime();
            bool isNeedReloadFromCist = !UniversityEntitiesRepository.IsInitialized && timePass > TimeSpan.FromDays(25);

            AddGroupPageViewModel = new AddGroupViewModel();
            AddTeacherPageViewModel = new AddTeacherViewModel();
            AddRoomPageViewModel = new AddRoomViewModel();

            if (isNeedReloadFromCist)
            {
                MainThread.BeginInvokeOnMainThread(BaseAddEntityViewModel<int>.UpdateFromCist);
            }
        }
    }
}
