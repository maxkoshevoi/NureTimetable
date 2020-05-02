using NureTimetable.Core.Localization;
using NureTimetable.DAL;
using NureTimetable.DAL.Models.Local;
using NureTimetable.Services.Helpers;
using NureTimetable.UI.ViewModels.Core;
using NureTimetable.UI.ViewModels.Lessons.ManageLessons;
using NureTimetable.UI.Views.Lessons;
using Plugin.DeviceInfo;
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
        public bool IsSelected
        {
            get => isSelected;
            set => SetProperty(ref isSelected, value, onChanged: () => _manageEntitiesViewModel.OnEntitySelectChange(this));
        }

        public bool IsMultiselectMode => _manageEntitiesViewModel.IsMultiselectMode;

        public ICommand SettingsClickedCommand { get; }

        public ICommand UpdateClickedCommand { get; }
        #endregion

        public SavedEntityItemViewModel(INavigation navigation, SavedEntity savedEntity, ManageEntitiesViewModel manageEntitiesViewModel) : base(navigation)
        {
            SavedEntity = savedEntity;
            _manageEntitiesViewModel = manageEntitiesViewModel;
            UpdateClickedCommand = CommandHelper.CreateCommand(UpdateClicked);
            SettingsClickedCommand = CommandHelper.CreateCommand(SettingsClicked);
        }

        public async Task UpdateClicked()
        {
            await _manageEntitiesViewModel.UpdateTimetable(SavedEntity);
        }

        public async Task SettingsClicked()
        {
            List<string> actionList = new List<string> { LN.UpdateTimetable, LN.SetUpLessonDisplay, LN.Delete };
            if (Device.RuntimePlatform == Device.Android
                && CrossDeviceInfo.Current.VersionNumber.Major > 0
                && CrossDeviceInfo.Current.VersionNumber.Major < 5)
            {
                // SfCheckBox doesn`t support Android 4
                actionList.Remove(LN.SetUpLessonDisplay);
            }
            if (!IsSelected)
            {
                actionList.Insert(0, LN.SelectOneEntity);
                actionList.Insert(1, LN.AddToSelected);
            }

            string action = await App.Current.MainPage.DisplayActionSheet(LN.ChooseAction, LN.Cancel, null, actionList.ToArray());
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
                await Navigation.PushAsync(new ManageLessonsPage()
                {
                    BindingContext = new ManageLessonsViewModel(Navigation, SavedEntity)
                });
            }
            else if (action == LN.Delete)
            {
                IsSelected = false;
                _manageEntitiesViewModel.Entities.Remove(this);
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
