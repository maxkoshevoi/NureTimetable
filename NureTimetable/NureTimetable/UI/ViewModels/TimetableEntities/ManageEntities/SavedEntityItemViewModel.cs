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
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace NureTimetable.UI.ViewModels.TimetableEntities.ManageEntities
{
    public class SavedEntityItemViewModel : BaseViewModel
    {
        private readonly ManageEntitiesViewModel manageEntitiesViewModel;

        #region Properties
        public SavedEntity SavedEntity { get; }

        private bool _isUpdating;
        public bool IsUpdating { get => _isUpdating; set { SetProperty(ref _isUpdating, value); manageEntitiesViewModel.UpdateAllCommand.RaiseCanExecuteChanged(); } }

        public bool IsMultiselectMode => manageEntitiesViewModel.IsMultiselectMode;

        public IAsyncCommand SettingsClickedCommand { get; }
        public IAsyncCommand UpdateClickedCommand { get; }
        #endregion

        public SavedEntityItemViewModel(SavedEntity savedEntity, ManageEntitiesViewModel manageEntitiesViewModel)
        {
            SavedEntity = savedEntity;
            this.manageEntitiesViewModel = manageEntitiesViewModel;

            UpdateClickedCommand = CommandHelper.Create(UpdateClicked);
            SettingsClickedCommand = CommandHelper.Create(SettingsClicked);
        }

        public Task UpdateClicked()
        {
            return manageEntitiesViewModel.UpdateTimetable(SavedEntity);
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
                manageEntitiesViewModel.SelectOne(SavedEntity);
                await Shell.Current.GoToAsync("//tabbar/Events");
            }
            else if (action == LN.AddToSelected)
            {
                SavedEntity.IsSelected = true;
            }
            else if (action == LN.UpdateTimetable)
            {
                await manageEntitiesViewModel.UpdateTimetable(SavedEntity);
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
                manageEntitiesViewModel.Entities.Remove(this);
                UniversityEntitiesRepository.UpdateSaved(manageEntitiesViewModel.Entities.Select(vm => vm.SavedEntity).ToList());
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
