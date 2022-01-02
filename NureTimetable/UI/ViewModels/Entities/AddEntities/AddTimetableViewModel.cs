using Microsoft.Maui.Controls;
using NureTimetable.Core.Localization;
using NureTimetable.DAL.Cist;
using NureTimetable.DAL.Settings;
using Xamarin.CommunityToolkit.ObjectModel;
using static NureTimetable.DAL.Cist.UniversityEntitiesRepository;

namespace NureTimetable.UI.ViewModels;

public class AddTimetableViewModel : BaseViewModel
{
    #region Properties
    public IAsyncCommand PageAppearingCommand { get; }
    public IAsyncCommand UpdateCommand { get; }

    private bool updateCommandEnabled = true;
    public bool UpdateCommandEnabled { get => updateCommandEnabled; set { updateCommandEnabled = value; UpdateCommand.RaiseCanExecuteChanged(); } }

    private string searchQuery = string.Empty;
    public string SearchQuery { get => searchQuery; set => SetProperty(ref searchQuery, value, onChanged: () => SearchEntities(value)); }

    public AddGroupViewModel AddGroupPageViewModel { get; } = new();
    public AddTeacherViewModel AddTeacherPageViewModel { get; } = new();
    public AddRoomViewModel AddRoomPageViewModel { get; } = new();
    #endregion

    public AddTimetableViewModel()
    {
        TimeSpan? timePass = DateTime.Now - SettingsRepository.Settings.LastCistAllEntitiesUpdate;
        bool isNeedReloadFromCist = !UniversityEntitiesRepository.IsInitialized && timePass > TimeSpan.FromDays(25);
        if (isNeedReloadFromCist)
        {
            Task.Run(UpdateFromCistAsync);
        }

        PageAppearingCommand = CommandFactory.Create(() => UpdateEntitiesOnAllTabs());
        UpdateCommand = CommandFactory.Create(UpdateEntities, () => UpdateCommandEnabled, allowsMultipleExecutions: false);
    }

    private void SearchEntities(string value)
    {
        AddGroupPageViewModel.SearchQueryChanged(value);
        AddTeacherPageViewModel.SearchQueryChanged(value);
        AddRoomPageViewModel.SearchQueryChanged(value);
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
            var updateFromCist = UniversityEntitiesRepository.UpdateFromCistAsync();
            await UpdateEntitiesOnAllTabs(updateFromCist);
            await DisplayUpdateResult(await updateFromCist);
        }
    }

    private async Task UpdateEntitiesOnAllTabs(Task? updateDataSource = null)
    {
        UpdateCommandEnabled = false;
        await Task.WhenAll(
            AddGroupPageViewModel.UpdateEntities(updateDataSource),
            AddTeacherPageViewModel.UpdateEntities(updateDataSource),
            AddRoomPageViewModel.UpdateEntities(updateDataSource)
        );
        UpdateCommandEnabled = true;
    }

    private static async Task DisplayUpdateResult(UniversityEntitiesCistUpdateResult updateResult)
    {
        string message;
        if (updateResult.IsAllFail)
        {
            message = LN.UniversityInfoUpdateFail;
            if (updateResult.IsConnectionIssues)
            {
                message = LN.CannotGetDataFromCist;
            }
            else if (updateResult.IsCistException)
            {
                message = LN.CistException;
            }
        }
        else if (!updateResult.IsAllSuccessful)
        {
            string failedEntities = Environment.NewLine;
            if (updateResult.GroupsException != null)
            {
                failedEntities += LN.Groups + Environment.NewLine;
            }
            if (updateResult.TeachersException != null)
            {
                failedEntities += LN.Teachers + Environment.NewLine;
            }
            if (updateResult.RoomsException != null)
            {
                failedEntities += LN.Rooms + Environment.NewLine;
            }
            message = string.Format(LN.UniversityInfoUpdatePartiallyFail, Environment.NewLine + failedEntities);
        }
        else
        {
            return;
        }

        await Shell.Current.DisplayAlert(LN.UniversityInfoUpdate, message, LN.Ok);
    }
}
