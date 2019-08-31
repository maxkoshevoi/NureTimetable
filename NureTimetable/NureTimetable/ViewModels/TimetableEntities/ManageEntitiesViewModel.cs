using Microsoft.AppCenter.Analytics;
using NureTimetable.Core.Localization;
using NureTimetable.Core.Models.Consts;
using NureTimetable.DAL;
using NureTimetable.DAL.Models.Local;
using NureTimetable.Services.Helpers;
using NureTimetable.UI.Views.TimetableEntities;
using NureTimetable.ViewModels.Core;
using NureTimetable.Views.Lessons;
using Plugin.DeviceInfo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace NureTimetable.ViewModels.TimetableEntities
{
    public class ManageEntitiesViewModel : BaseViewModel
    {
        #region Classes
        public class SavedEntityItemViewModel
        {
            #region variables
            private ManageEntitiesViewModel _manageEntitiesViewModel;
            #endregion

            #region Properties
            public SavedEntity SavedEntity { get; }

            // TODO: Move this property inside SavedEntity and replace SelectedEntity functionality with it
            public bool IsSelected { get; set; }

            public ICommand SettingsClickedCommand { get; }

            public ICommand UpdateClickedCommand { get; }
            #endregion

            public SavedEntityItemViewModel(SavedEntity savedEntity, ManageEntitiesViewModel manageEntitiesViewModel)
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
                if (!IsSelected)
                {
                    actionList.Insert(0, LN.Select);
                }
                if (Device.RuntimePlatform == Device.Android 
                    && CrossDeviceInfo.Current.VersionNumber.Major > 0 
                    && CrossDeviceInfo.Current.VersionNumber.Major < 5)
                {
                    // SfCheckBox doesn`t support Android 4
                    actionList.Remove(LN.SetUpLessonDisplay);
                }

                string action = await App.Current.MainPage.DisplayActionSheet(LN.ChooseAction, LN.Cancel, null, actionList.ToArray());
                if (action == LN.Select)
                {
                    UniversityEntitiesRepository.UpdateSelected(SavedEntity);
                    await _manageEntitiesViewModel.Navigation.PopToRootAsync();
                }
                else if (action == LN.UpdateTimetable)
                {
                    await _manageEntitiesViewModel.UpdateTimetable(SavedEntity);
                }
                else if (action == LN.SetUpLessonDisplay)
                {
                    await _manageEntitiesViewModel.Navigation.PushAsync(new ManageLessonsPage(SavedEntity));
                }
                else if (action == LN.Delete)
                {
                    _manageEntitiesViewModel.Entities.Remove(this);
                    UniversityEntitiesRepository.UpdateSaved(_manageEntitiesViewModel.Entities.Select(vm => vm.SavedEntity).ToList());
                }
            }
        }
        #endregion

        #region variables
        private bool _isNoSourceLayoutVisable;

        private bool _isEntitiesLayoutEnable;

        private bool _isProgressVisable;

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

        private SavedEntityItemViewModel _savedEntitySelectedItem;

        public SavedEntityItemViewModel SavedEntitySelectedItem
        {
            get => _savedEntitySelectedItem;
            set
            {
                if (value != null)
                {
                    Device.BeginInvokeOnMainThread(async () => { await SavedEntitySelected(value); });
                }

                _savedEntitySelectedItem = value;
            }
        }

        public ObservableCollection<SavedEntityItemViewModel> Entities { get => _entities; set => SetProperty(ref _entities, value); }

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
        public async Task SavedEntitySelected(SavedEntityItemViewModel selectedEntity)
        {
            if (!selectedEntity.IsSelected)
            {
                UniversityEntitiesRepository.UpdateSelected(selectedEntity.SavedEntity);
            }
            await Navigation.PopToRootAsync();
        }

        private async Task UpdateAll()
        {
            if (!IsEntitiesLayoutEnabled)
            {
                return;
            }
            if (await App.Current.MainPage.DisplayAlert(LN.TimetableUpdate, LN.UpdateAllTimetables, LN.Yes, LN.Cancel))
            {
                await UpdateTimetable(Entities?.Select(vm => vm.SavedEntity).ToArray());
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

            SavedEntity selectedEntity = UniversityEntitiesRepository.GetSelected();
            Entities = new ObservableCollection<SavedEntityItemViewModel>(
                newItems.Select(sg => new SavedEntityItemViewModel(sg, this)
                {
                    IsSelected = sg.ID == selectedEntity?.ID
                })
            );
        }

        private async Task UpdateTimetable(params SavedEntity[] entities)
        {
            if (entities == null || entities.Length == 0)
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
                        await Navigation.PopToRootAsync();
                    }
                });
            });
        }
        #endregion
    }
}