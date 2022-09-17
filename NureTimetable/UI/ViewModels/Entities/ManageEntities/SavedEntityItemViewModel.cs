using NureTimetable.UI.Models.Consts;

namespace NureTimetable.UI.ViewModels;

public class SavedEntityItemViewModel : BaseViewModel
{
    #region Properties
    public ManageEntitiesViewModel ManageEntitiesViewModel { get; }

    public SavedEntity SavedEntity { get; }

    private bool _isUpdating;
    public bool IsUpdating { get => _isUpdating; set { SetProperty(ref _isUpdating, value); ManageEntitiesViewModel.UpdateAllCommand.NotifyCanExecuteChanged(); } }

    public IRelayCommand SettingsClickedCommand { get; }
    public IRelayCommand UpdateClickedCommand { get; }
    #endregion

    public SavedEntityItemViewModel(SavedEntity savedEntity, ManageEntitiesViewModel manageEntitiesViewModel)
    {
        SavedEntity = savedEntity;
        ManageEntitiesViewModel = manageEntitiesViewModel;

        var existingEntity = manageEntitiesViewModel.Entities.Where(se => se.SavedEntity == savedEntity).DefaultIfEmpty().Single();
        if (existingEntity != null)
        {
            IsUpdating = existingEntity.IsUpdating;
        }

        UpdateClickedCommand = CommandFactory.Create(() => TimetableService.UpdateAndDisplayResultAsync(SavedEntity));
        SettingsClickedCommand = CommandFactory.Create(SettingsClicked);
    }

    public async Task SettingsClicked()
    {
        List<string> actionList = new() { LN.UpdateTimetable, LN.SetUpLessonDisplay, LN.Delete };
        if (!SavedEntity.IsSelected)
        {
            actionList.Insert(0, LN.SelectOneEntity);
            actionList.Insert(1, LN.AddToSelected);
        }

        string action = await Shell.Current.DisplayActionSheet(LN.ChooseAction, LN.Cancel, null, actionList.ToArray());
        if (action == LN.SelectOneEntity)
        {
            await ManageEntitiesViewModel.SelectOne(SavedEntity);
            await Shell.Current.GoToAsync(Route.EventsTab);
        }
        else if (action == LN.AddToSelected)
        {
            SavedEntity.IsSelected = true;
        }
        else if (action == LN.UpdateTimetable)
        {
            await TimetableService.UpdateAndDisplayResultAsync(SavedEntity);
        }
        else if (action == LN.SetUpLessonDisplay)
        {
            await Navigation.PushAsync(new ManageLessonsPage
            {
                BindingContext = new ManageLessonsViewModel(SavedEntity)
            });
        }
        else if (action == LN.Delete)
        {
            await UniversityEntitiesRepository.ModifySavedAsync(savedEntities => !savedEntities.Remove(SavedEntity));
        }
    }
}
