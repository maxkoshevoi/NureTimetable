using NureTimetable.Core.Localization;
using NureTimetable.DAL;
using NureTimetable.UI.Helpers;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace NureTimetable.UI.ViewModels.TimetableEntities
{
    public class AddTimetableViewModel : BaseViewModel
    {
        #region Properties
        public Command UpdateCommand { get; }
        private bool updateCommandEnabled = true;
        public bool UpdateCommandEnabled { get => updateCommandEnabled; set { updateCommandEnabled = value; UpdateCommand.ChangeCanExecute(); } }

        public AddGroupViewModel AddGroupPageViewModel { get; }
        public AddTeacherViewModel AddTeacherPageViewModel { get; }
        public AddRoomViewModel AddRoomPageViewModel { get; }
        #endregion

        public AddTimetableViewModel()
        {
            TimeSpan? timePass = DateTime.Now - SettingsRepository.GetLastCistAllEntitiesUpdateTime();
            bool isNeedReloadFromCist = !UniversityEntitiesRepository.IsInitialized && timePass > TimeSpan.FromDays(25);
            if (isNeedReloadFromCist)
            {
                Task.Run(UpdateFromCist);
            }

            AddGroupPageViewModel = new AddGroupViewModel();
            AddTeacherPageViewModel = new AddTeacherViewModel();
            AddRoomPageViewModel = new AddRoomViewModel();

            UpdateCommand = CommandHelper.Create(UpdateEntities, () => UpdateCommandEnabled);
        }

        private async Task UpdateEntities()
        {
            if (SettingsRepository.CheckCistAllEntitiesUpdateRights() == false)
            {
                await Shell.Current.DisplayAlert(LN.UniversityInfoUpdate, LN.UniversityInfoUpToDate, LN.Ok);
                return;
            }

            if (await Shell.Current.DisplayAlert(LN.UniversityInfoUpdate, LN.UniversityInfoUpdateConfirm, LN.Yes, LN.Cancel))
            {
                UpdateCommandEnabled = false;
                var updateFromCist = Task.Run(UpdateFromCist);
                await Task.WhenAll(
                    AddGroupPageViewModel.UpdateEntities(updateFromCist),
                    AddTeacherPageViewModel.UpdateEntities(updateFromCist),
                    AddRoomPageViewModel.UpdateEntities(updateFromCist)
                );
                UpdateCommandEnabled = true;
            }
        }

        public static async Task UpdateFromCist()
        {
            var updateFromCistResult = UniversityEntitiesRepository.UpdateFromCist();

            if (updateFromCistResult.IsAllFail)
            {
                string message = LN.UniversityInfoUpdateFail;
                if (updateFromCistResult.IsConnectionIssues)
                {
                    message = LN.CannotGetDataFromCist;
                }
                else if (updateFromCistResult.IsCistOutOfMemory)
                {
                    message = LN.CistOutOfMemory;
                }
                await MainThread.InvokeOnMainThreadAsync(async () =>
                    await Shell.Current.DisplayAlert(LN.UniversityInfoUpdate, message, LN.Ok)
                );
            }
            else if (!updateFromCistResult.IsAllSuccessful)
            {
                string failedEntities = Environment.NewLine;
                if (updateFromCistResult.GroupsException != null)
                {
                    failedEntities += LN.Groups + Environment.NewLine;
                }
                if (updateFromCistResult.TeachersException != null)
                {
                    failedEntities += LN.Teachers + Environment.NewLine;
                }
                if (updateFromCistResult.RoomsException != null)
                {
                    failedEntities += LN.Rooms + Environment.NewLine;
                }
                await MainThread.InvokeOnMainThreadAsync(async () =>
                    await Shell.Current.DisplayAlert(LN.UniversityInfoUpdate, string.Format(LN.UniversityInfoUpdatePartiallyFail, Environment.NewLine + failedEntities), LN.Ok)
                );
            }
        }
    }
}
