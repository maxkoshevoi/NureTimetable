using Microsoft.AppCenter.Analytics;
using NureTimetable.Core.Localization;
using NureTimetable.Core.Models.Consts;
using NureTimetable.DAL;
using NureTimetable.DAL.Models.Local;
using NureTimetable.Services.Helpers;
using NureTimetable.UI.ViewModels.Core;
using NureTimetable.UI.Views.Lessons;
using NureTimetable.UI.Views.TimetableEntities;
using Plugin.DeviceInfo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace NureTimetable.UI.ViewModels.TimetableEntities
{
    public class ManageEntitiesViewModel : BaseViewModel
    {
        #region Classes
        public class SavedEntityItemViewModel : BaseViewModel
        {
            #region variables
            private ManageEntitiesViewModel _manageEntitiesViewModel;
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
                    await _manageEntitiesViewModel.Navigation.PushAsync(new ManageLessonsPage()
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
        #endregion

        #region Variables
        private bool _isNoSourceLayoutVisable;

        private bool _isEntitiesLayoutEnable;

        private bool _isProgressVisable;

        private bool _isMultiselectMode;

        private SavedEntityItemViewModel _selectedEntity;

        private ObservableCollection<SavedEntityItemViewModel> _entities;
        #endregion

        #region Properties
        public bool IsNoSourceLayoutVisable
        {
            get => _isNoSourceLayoutVisable;
            set => SetProperty(ref _isNoSourceLayoutVisable, value);
        }

        public bool IsProgressVisable
        {
            get => _isProgressVisable;
            set => SetProperty(ref _isProgressVisable, value);
        }

        public bool IsEntitiesLayoutEnabled
        {
            get => _isEntitiesLayoutEnable;
            set => SetProperty(ref _isEntitiesLayoutEnable, value);
        }

        public bool IsMultiselectMode
        {
            get => _isMultiselectMode;
            set => SetProperty(ref _isMultiselectMode, value, onChanged: () => Entities.ForEach(e => e.NotifyChanged(nameof(IsMultiselectMode))));
        }

        public SavedEntityItemViewModel SelectedEntity { get => _selectedEntity; set => SetProperty(ref _selectedEntity, value, onChanged: SavedEntitySelected); }

        public ObservableCollection<SavedEntityItemViewModel> Entities { get => _entities; private set => SetProperty(ref _entities, value); }

        public ICommand UpdateAllCommand { get; }

        public ICommand AddEntityCommand { get; }

        #endregion

        public ManageEntitiesViewModel(INavigation navigation) : base(navigation)
        {
            IsProgressVisable = false;
            IsEntitiesLayoutEnabled = true;

            UpdateItems(UniversityEntitiesRepository.GetSaved());
            MessagingCenter.Subscribe<Application, List<SavedEntity>>(this, MessageTypes.SavedEntitiesChanged, (sender, newSavedEntities) =>
            {
                UpdateItems(newSavedEntities);
            });

            UpdateAllCommand = CommandHelper.CreateCommand(UpdateAll);
            AddEntityCommand = CommandHelper.CreateCommand(AddEntity);
        }

        #region Methods
        public async void SavedEntitySelected()
        {
            if (SelectedEntity == null)
            {
                return;
            }

            if (UniversityEntitiesRepository.GetSelected().Count > 1)
            {
                SelectedEntity.IsSelected = !SelectedEntity.IsSelected;
            }
            else
            {
                await SelectOneAndExit(SelectedEntity.SavedEntity);
            }
        }

        public async Task SelectOneAndExit(SavedEntity savedEntity)
        {
            UniversityEntitiesRepository.UpdateSelected(savedEntity);
            await Navigation.PopToRootAsync();
        }

        public void OnEntitySelectChange(SavedEntityItemViewModel entity)
        {
            List<SavedEntity> currentSelected = UniversityEntitiesRepository.GetSelected();
            if (entity.IsSelected)
            {
                if (currentSelected.Contains(entity.SavedEntity))
                {
                    return;
                }
                currentSelected.Add(entity.SavedEntity);
            }
            else
            {
                if (!currentSelected.Contains(entity.SavedEntity))
                {
                    return;
                }
                if (currentSelected.Count == 1)
                {
                    // User cannot deselect last selected entity
                    entity.IsSelected = true;
                    return;
                }
                currentSelected.Remove(entity.SavedEntity);
            }
            UniversityEntitiesRepository.UpdateSelected(currentSelected);

            // Changing multiselect state
            IsMultiselectMode = currentSelected.Count > 1;
        }

        private async Task UpdateAll()
        {
            if (!IsEntitiesLayoutEnabled)
            {
                return;
            }
            if (await App.Current.MainPage.DisplayAlert(LN.TimetableUpdate, LN.UpdateAllTimetables, LN.Yes, LN.Cancel))
            {
                await UpdateTimetable(Entities?.Select(vm => vm.SavedEntity).ToList());
            }
        }

        private async Task AddEntity()
        {
            if (!IsEntitiesLayoutEnabled)
            {
                return;
            }
            await Navigation.PushAsync(new AddTimetablePage()
            {
                BindingContext = new AddTimetableViewModel(Navigation)
            });
        }

        private void UpdateItems(List<SavedEntity> newItems)
        {
            IsNoSourceLayoutVisable = (newItems.Count == 0);

            List<SavedEntity> selectedEntities = UniversityEntitiesRepository.GetSelected();
            Entities = new ObservableCollection<SavedEntityItemViewModel>(
                newItems.Select(sg => new SavedEntityItemViewModel(Navigation, sg, this)
                {
                    IsSelected = selectedEntities.Any(se => se.ID == sg.ID)
                })
            );
            IsMultiselectMode = selectedEntities.Count > 1;
        }

        private Task UpdateTimetable(SavedEntity entity)
            => UpdateTimetable(new List<SavedEntity>() { entity });

        private async Task UpdateTimetable(List<SavedEntity> entities)
        {
            if (entities == null || entities.Count == 0)
            {
                return;
            }

            List<SavedEntity> entitiesAllowed = SettingsRepository.CheckCistTimetableUpdateRights(entities);
            if (entitiesAllowed.Count == 0)
            {
                await App.Current.MainPage.DisplayAlert(LN.TimetableUpdate, LN.TimetableLatest, LN.Ok);
                return;
            }

            IsEntitiesLayoutEnabled = false;
            IsProgressVisable = true;

            await Task.Factory.StartNew(() =>
            {
#if !DEBUG
                Analytics.TrackEvent("Updating timetable", new Dictionary<string, string>
                {
                    { "Count", entitiesAllowed.Count.ToString() },
                    { "Hour of the day", DateTime.Now.Hour.ToString() }
                });
#endif

                List<string> success = new List<string>(), fail = new List<string>();
                foreach (SavedEntity entity in entitiesAllowed)
                {
                    if (EventsRepository.GetTimetableFromCist(entity, Config.TimetableFromDate, Config.TimetableToDate) != null)
                    {
                        success.Add(entity.Name);
                    }
                    else
                    {
                        fail.Add(entity.Name);
                    }
                }
                string result = "";
                if (success.Count > 0)
                {
                    result += string.Format(LN.TimetableUpdated, string.Join(", ", success) + Environment.NewLine);
                }
                if (fail.Count > 0)
                {
                    result += string.Format(LN.ErrorOccurred, string.Join(", ", fail));
                }

                Device.BeginInvokeOnMainThread(async () =>
                {
                    IsProgressVisable = false;
                    IsEntitiesLayoutEnabled = true;
                    if (await App.Current.MainPage.DisplayAlert(LN.TimetableUpdate, result, LN.ToTimetable, LN.Ok))
                    {
                        List<SavedEntity> selected = UniversityEntitiesRepository.GetSelected();
                        if (entitiesAllowed.Count == 1 && !selected.Contains(entitiesAllowed[0]))
                        {
                            await SelectOneAndExit(entitiesAllowed[0]);
                        }
                        else
                        {
                            await Navigation.PopToRootAsync();
                        }
                    }
                });
            });
        }
        #endregion
    }
}