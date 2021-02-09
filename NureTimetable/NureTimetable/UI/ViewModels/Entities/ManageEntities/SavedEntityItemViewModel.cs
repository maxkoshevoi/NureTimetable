using NureTimetable.BL;
using NureTimetable.Core.Localization;
using NureTimetable.DAL;
using NureTimetable.DAL.Models.Local;
using NureTimetable.UI.Helpers;
using NureTimetable.UI.ViewModels.Lessons.ManageLessons;
using NureTimetable.UI.Views.Lessons;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace NureTimetable.UI.ViewModels.Entities.ManageEntities
{
    public class SavedEntityItemViewModel : BaseViewModel
    {
        #region Properties
        public ManageEntitiesViewModel ManageEntitiesViewModel { get; }

        public SavedEntity SavedEntity { get; }

        private bool _isUpdating;
        public bool IsUpdating { get => _isUpdating; set { SetProperty(ref _isUpdating, value); ManageEntitiesViewModel.UpdateAllCommand.RaiseCanExecuteChanged(); } }

        public IAsyncCommand SettingsClickedCommand { get; }
        public IAsyncCommand UpdateClickedCommand { get; }
        #endregion

        public SavedEntityItemViewModel(SavedEntity savedEntity, ManageEntitiesViewModel manageEntitiesViewModel)
        {
            SavedEntity = savedEntity;
            ManageEntitiesViewModel = manageEntitiesViewModel;

            var existingEntity = manageEntitiesViewModel.Entities.SingleOrDefault(se => se.SavedEntity == savedEntity);
            if (existingEntity != null)
            {
                IsUpdating = existingEntity.IsUpdating;
            }

            UpdateClickedCommand = CommandFactory.Create(() => TimetableService.UpdateAndDisplayResult(SavedEntity));
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
                ManageEntitiesViewModel.SelectOne(SavedEntity);
                await Shell.Current.GoToAsync("//tabbar/Events");
            }
            else if (action == LN.AddToSelected)
            {
                SavedEntity.IsSelected = true;
            }
            else if (action == LN.UpdateTimetable)
            {
                await TimetableService.UpdateAndDisplayResult(SavedEntity);
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
                ManageEntitiesViewModel.Entities.Remove(this);
                UniversityEntitiesRepository.UpdateSaved(ManageEntitiesViewModel.Entities.Select(vm => vm.SavedEntity).ToList());
            }
        }
    }
}
