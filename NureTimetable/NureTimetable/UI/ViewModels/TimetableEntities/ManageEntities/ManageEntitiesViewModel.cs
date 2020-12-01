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
        public bool IsNoSourceLayoutVisable { get => _isNoSourceLayoutVisable; set => SetProperty(ref _isNoSourceLayoutVisable, value); }

        public bool IsMultiselectMode
        {
            get => _isMultiselectMode;
            set => SetProperty(ref _isMultiselectMode, value, () => Entities?.ForEach(e => e.NotifyChanged(nameof(IsMultiselectMode))));
        }

        public ObservableCollection<SavedEntityItemViewModel> Entities { get => _entities; private set => SetProperty(ref _entities, value); }

        public Command UpdateAllCommand { get; }
        public Command AddEntityCommand { get; }
        public Command EntitySelectedCommand { get; }
        #endregion

        public ManageEntitiesViewModel()
        {
            UpdateAllCommand = CommandHelper.Create(UpdateAll, () => Entities.Any() && Entities.All(e => !e.IsUpdating));
            AddEntityCommand = CommandHelper.Create(AddEntity);
            EntitySelectedCommand = CommandHelper.Create<SelectedItemChangedEventArgs>(async (args) =>
            {
                if (args.SelectedItem is not SavedEntityItemViewModel entity) return;

                SavedEntity savedEntity = entity.SavedEntity;
                if (IsMultiselectMode)
                {
                    savedEntity.IsSelected = !savedEntity.IsSelected;
                }
                else
                {
                    SelectOne(savedEntity);
                    await Shell.Current.GoToAsync("//tabbar/Events");
                }
            });

            UpdateItems();
            MessagingCenter.Subscribe<Application, List<SavedEntity>>(this, MessageTypes.SavedEntitiesChanged, (sender, newSavedEntities) =>
            {
                UpdateItems(newSavedEntities);
            });
            MessagingCenter.Subscribe<Application, Entity>(this, MessageTypes.TimetableUpdating, (sender, entity) =>
            {
                SavedEntityItemViewModel savedEntity = Entities.SingleOrDefault(e => e.SavedEntity == entity);
                if (savedEntity is not null)
                    savedEntity.IsUpdating = true;
            });
            MessagingCenter.Subscribe<Application, Entity>(this, MessageTypes.TimetableUpdated, (sender, entity) =>
            {
                SavedEntityItemViewModel savedEntity = Entities.SingleOrDefault(e => e.SavedEntity == entity);
                if (savedEntity is not null)
                    savedEntity.IsUpdating = false;
            });
        }

        #region Methods
        public void SelectOne(SavedEntity savedEntity)
        {
            List<SavedEntity> savedEntities = UniversityEntitiesRepository.GetSaved();
            foreach (SavedEntity e in savedEntities)
            {
                e.IsSelected = e == savedEntity;
            }
            UniversityEntitiesRepository.UpdateSaved(savedEntities);
            UpdateItems(savedEntities);
        }

        public void EntitySelectChanged(SavedEntityItemViewModel entity)
        {
            List<SavedEntity> currentSaved = UniversityEntitiesRepository.GetSaved();
            SavedEntity savedEntity = currentSaved.SingleOrDefault(e => e == entity.SavedEntity);

            // Check state is changed
            if (savedEntity.IsSelected == entity.SavedEntity.IsSelected)
            {
                return;
            }
            savedEntity.IsSelected = entity.SavedEntity.IsSelected;

            // If deselecting last selected entity
            if (!savedEntity.IsSelected && !currentSaved.Any(e => e.IsSelected))
            {
                if (Entities.Contains(entity))
                {
                    // User cannot deselect last selected entity
                    savedEntity.IsSelected = true;
                    return;
                }

                var otherSavedEntity = Entities.FirstOrDefault();
                if (otherSavedEntity != null)
                {
                    // If user deleted last selected entity, selecting any other saved entity
                    otherSavedEntity.SavedEntity.IsSelected = true;
                    currentSaved = UniversityEntitiesRepository.GetSaved();
                }
            }

            UniversityEntitiesRepository.UpdateSaved(currentSaved);

            // Changing multiselect state
            IsMultiselectMode = currentSaved.Count(e => e.IsSelected) > 1;
        }

        private async Task UpdateAll()
        {
            if (await Shell.Current.DisplayAlert(LN.TimetableUpdate, LN.UpdateAllTimetables, LN.Yes, LN.Cancel))
            {
                await UpdateTimetable(Entities?.Select(vm => (Entity)vm.SavedEntity).ToList());
            }
        }

        private async Task AddEntity()
        {
            await Navigation.PushAsync(new AddTimetablePage());
        }

        private void UpdateItems()
            => UpdateItems(UniversityEntitiesRepository.GetSaved());

        private void UpdateItems(List<SavedEntity> newItems)
        {
            IsNoSourceLayoutVisable = (newItems.Count == 0);

            Entities = new ObservableCollection<SavedEntityItemViewModel>(
                newItems.Select(sg =>
                {
                    SavedEntityItemViewModel displaysEntity = Entities?.SingleOrDefault(e => e.SavedEntity == sg);
                    if (displaysEntity is null)
                    {
                        displaysEntity = new SavedEntityItemViewModel(sg, this);
                        displaysEntity.SavedEntity.PropertyChanged += (sender, args) =>
                        {
                            if (args.PropertyName == nameof(SavedEntity.IsSelected))
                                EntitySelectChanged(displaysEntity);
                        };
                    }

                    displaysEntity.SavedEntity.IsSelected = sg.IsSelected;
                    displaysEntity.SavedEntity.LastUpdated = sg.LastUpdated;
                    return displaysEntity;
                })
            );

            IsMultiselectMode = newItems.Count(i => i.IsSelected) > 1;
            UpdateAllCommand.ChangeCanExecute();
        }

        public Task UpdateTimetable(Entity entity)
            => UpdateTimetable(new List<Entity>() { entity });

        private async Task UpdateTimetable(List<Entity> entities)
        {
            if (entities is null || !entities.Any())
            {
                return;
            }

            List<Entity> entitiesAllowed = SettingsRepository.CheckCistTimetableUpdateRights(entities);
            if (entitiesAllowed.Count == 0)
            {
                await Shell.Current.DisplayAlert(LN.TimetableUpdate, LN.TimetableLatest, LN.Ok);
                return;
            }

            Analytics.TrackEvent("Updating timetable", new Dictionary<string, string>
            {
                { "Count", entitiesAllowed.Count.ToString() },
                { "Hour of the day", DateTime.Now.Hour.ToString() }
            });

            // Update timetables in background
            const int batchSize = 10;
            List<Task<(TimetableInfo _, Exception Error)>> updateTasks = new();
            for (int i = 0; i < entitiesAllowed.Count; i += batchSize)
            {
                foreach (Entity entity in entitiesAllowed.Skip(i).Take(batchSize))
                {
                    updateTasks.Add(EventsRepository.GetTimetableFromCist(entity, Config.TimetableFromDate, Config.TimetableToDate));
                }
                await Task.WhenAll(updateTasks);
            }

            List<string> success = new(), fail = new();
            bool isNetworkError = false;
            bool isCistError = false;
            for (int i = 0; i < updateTasks.Count; i++)
            {
                Exception ex = updateTasks[i].Result.Error;
                Entity entity = entitiesAllowed[i];
                if (ex is null)
                {
                    success.Add(entity.Name);
                    continue;
                }

                if (ex is WebException)
                {
                    isNetworkError = true;
                }
                else if (ex is CistException)
                {
                    isCistError = true;
                }

                string errorMessage = ex.Message;
                if (errorMessage.Length > 30)
                {
                    errorMessage = errorMessage.Remove(30);
                }
                fail.Add($"{entity.Name} ({errorMessage.Trim()})");
            }

            if (success.Count == entitiesAllowed.Count)
            {
                return;
            }

            string result = string.Empty;
            if (isNetworkError && fail.Count == entitiesAllowed.Count)
            {
                result = LN.CannotGetDataFromCist;
            }
            else if (isCistError)
            {
                result = LN.CistException;
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

            if (await Shell.Current.DisplayAlert(LN.TimetableUpdate, result, LN.ToTimetable, LN.Ok))
            {
                SavedEntity firstAllowedEntity = UniversityEntitiesRepository.GetSaved().Single(e => e == entitiesAllowed.First());
                if (entitiesAllowed.Count == 1 && !firstAllowedEntity.IsSelected)
                {
                    SelectOne(firstAllowedEntity);
                }

                await Shell.Current.GoToAsync("//tabbar/Events");
            }
        }
        #endregion
    }
}