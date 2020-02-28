using Microsoft.AppCenter.Analytics;
using NureTimetable.Core.Localization;
using NureTimetable.Core.Models.Consts;
using NureTimetable.DAL;
using NureTimetable.DAL.Models.Local;
using NureTimetable.Services.Helpers;
using NureTimetable.UI.ViewModels.Core;
using NureTimetable.UI.Views.TimetableEntities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace NureTimetable.UI.ViewModels.TimetableEntities.ManageEntities
{
    public class ManageEntitiesViewModel : BaseViewModel
    {
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
            set => SetProperty(ref _isMultiselectMode, value, onChanged: () => Entities?.ForEach(e => e.NotifyChanged(nameof(IsMultiselectMode))));
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

        public Task UpdateTimetable(SavedEntity entity)
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