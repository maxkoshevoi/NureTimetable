using Microsoft.AppCenter.Analytics;
using NureTimetable.Core.Localization;
using NureTimetable.Core.Models.Consts;
using NureTimetable.Core.Models.Exceptions;
using NureTimetable.DAL;
using NureTimetable.DAL.Models.Local;
using NureTimetable.UI.Helpers;
using NureTimetable.UI.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
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

        public ManageEntitiesViewModel()
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
            if (SelectedEntity is null)
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
            if (currentSelected.Contains(entity.SavedEntity) == entity.IsSelected)
            {
                return;
            }

            if (entity.IsSelected)
            {
                currentSelected.Add(entity.SavedEntity);
            }
            else
            {
                if (currentSelected.Count == 1)
                {
                    if (Entities.Contains(entity))
                    {
                        // User cannot deselect last selected entity
                        entity.IsSelected = true;
                        return;
                    }

                    var otherSavedEntity = Entities.FirstOrDefault();
                    if (otherSavedEntity != null)
                    {
                        otherSavedEntity.IsSelected = true;
                        currentSelected = UniversityEntitiesRepository.GetSelected();
                    }
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
            await Navigation.PushAsync(new AddTimetablePage());
        }

        private void UpdateItems(List<SavedEntity> newItems)
        {
            IsNoSourceLayoutVisable = (newItems.Count == 0);

            List<SavedEntity> selectedEntities = UniversityEntitiesRepository.GetSelected();
            Entities = new ObservableCollection<SavedEntityItemViewModel>(
                newItems.Select(sg => new SavedEntityItemViewModel(sg, this)
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
            if (entities is null || !entities.Any())
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

            Analytics.TrackEvent("Updating timetable", new Dictionary<string, string>
            {
                { "Count", entitiesAllowed.Count.ToString() },
                { "Hour of the day", DateTime.Now.Hour.ToString() }
            });

            const int batchSize = 10;
            var updateTasks = new List<Task<(TimetableInfo _, Exception Exception)>>();
            for (int i = 0; i < entitiesAllowed.Count; i += batchSize)
            {
                foreach (SavedEntity entity in entitiesAllowed.Skip(i).Take(batchSize))
                {
                    updateTasks.Add(EventsRepository.GetTimetableFromCist(entity, Config.TimetableFromDate, Config.TimetableToDate));
                }
                await Task.WhenAll(updateTasks);
            }
            
            List<string> success = new List<string>(), fail = new List<string>();
            bool isNetworkError = false;
            bool isCistOutOfMemoryError = false;
            for (int i = 0; i < updateTasks.Count; i++)
            {
                Exception ex = updateTasks[i].Result.Exception;
                SavedEntity entity = entitiesAllowed[i];
                if (ex is null)
                {
                    success.Add(entity.Name);
                    continue;
                }

                if (ex is WebException)
                {
                    isNetworkError = true;
                }
                else if (ex is CistOutOfMemoryException)
                {
                    isCistOutOfMemoryError = true;
                }

                string errorMessage = ex.Message;
                if (errorMessage.Length > 30)
                {
                    errorMessage = errorMessage.Remove(30);
                }
                fail.Add($"{entity.Name} ({errorMessage.Trim()})");
            }

            string result = "";
            if (isNetworkError && fail.Count == entitiesAllowed.Count)
            {
                result = LN.CannotGetDataFromCist;
            }
            else if (isCistOutOfMemoryError)
            {
                result = LN.CistOutOfMemory;
            }
            else
            {
                if (success.Count > 0)
                {
                    result += string.Format(LN.TimetableUpdated, string.Join(", ", success) + Environment.NewLine);
                }
                if (fail.Count > 0)
                {
                    result += string.Format(LN.ErrorOccurred, string.Join(", ", fail));
                }
            }

            IsProgressVisable = false;
            IsEntitiesLayoutEnabled = true;
            if (await App.Current.MainPage.DisplayAlert(LN.TimetableUpdate, result, LN.ToTimetable, LN.Ok))
            {
#pragma warning disable CS4014 // TimetableViewModel.PageAppearing wouldn't trigger if we use await. See issue #35
                List<SavedEntity> selected = UniversityEntitiesRepository.GetSelected();
                if (entitiesAllowed.Count == 1 && !selected.Contains(entitiesAllowed[0]))
                {
                    SelectOneAndExit(entitiesAllowed[0]);
                }
                else
                {
                    Navigation.PopToRootAsync();
                }
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }
        #endregion
    }
}