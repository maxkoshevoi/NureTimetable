using NureTimetable.Core.Localization;
using NureTimetable.DAL;
using NureTimetable.DAL.Models.Local;
using NureTimetable.UI.Helpers;
using NureTimetable.UI.ViewModels.Lessons.ManageLessons;
using NureTimetable.UI.Views.Lessons;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace NureTimetable.UI.ViewModels.TimetableEntities.ManageEntities
{
    public class SavedEntityItemViewModel : BaseViewModel
    {
        #region variables
        private readonly ManageEntitiesViewModel _manageEntitiesViewModel;
        #endregion

        #region Properties
        public SavedEntity SavedEntity { get; }

        // TODO: Move this property inside SavedEntity and replace SelectedEntity functionality with it
        private bool isSelected;
        public bool IsSelected { get => isSelected; set => SetProperty(ref isSelected, value, () => _manageEntitiesViewModel.OnEntitySelectChange(this)); }

        private bool isUpdating;
        public bool IsUpdating { get => isUpdating; set { SetProperty(ref isUpdating, value); _manageEntitiesViewModel.UpdateAllCommand.ChangeCanExecute(); } }

        public bool IsMultiselectMode => _manageEntitiesViewModel.IsMultiselectMode;

        public ICommand SettingsClickedCommand { get; }
        public ICommand UpdateClickedCommand { get; }
        #endregion

        public SavedEntityItemViewModel(SavedEntity savedEntity, ManageEntitiesViewModel manageEntitiesViewModel)
        {
            SavedEntity = savedEntity;
            _manageEntitiesViewModel = manageEntitiesViewModel;
            UpdateClickedCommand = CommandHelper.Create(UpdateClicked);
            SettingsClickedCommand = CommandHelper.Create(SettingsClicked);
        }

        public Task UpdateClicked()
        {
            return _manageEntitiesViewModel.UpdateTimetable(SavedEntity);
        }

        public async Task SettingsClicked()
        {
            var actionList = new List<string> { LN.UpdateTimetable, LN.SetUpLessonDisplay, LN.Delete };
            if (!IsSelected)
            {
                actionList.Insert(0, LN.SelectOneEntity);
                actionList.Insert(1, LN.AddToSelected);
            }

            string action = await Shell.Current.DisplayActionSheet(LN.ChooseAction, LN.Cancel, null, actionList.ToArray());
            if (action == LN.SelectOneEntity)
            {
                await _manageEntitiesViewModel.SelectOneAndExit(SavedEntity);
            }
            else if (action == LN.AddToSelected)
            {
                IsSelected = true;
            }
            else if (action == LN.UpdateTimetable)
            {
                await _manageEntitiesViewModel.UpdateTimetable(SavedEntity);
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
                _manageEntitiesViewModel.Entities.Remove(this);
                IsSelected = false;
                UniversityEntitiesRepository.UpdateSaved(_manageEntitiesViewModel.Entities.Select(vm => vm.SavedEntity).ToList());
            }
        }

        #region INotifyPropertyChanged
        public void NotifyChanged(string property)
        {
            OnPropertyChanged(property);
        }
        #endregion
    }
}
