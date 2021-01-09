using NureTimetable.BL;
using NureTimetable.Core.Localization;
using NureTimetable.Core.Models.Consts;
using NureTimetable.DAL;
using NureTimetable.DAL.Models.Local;
using NureTimetable.UI.Helpers;
using NureTimetable.UI.Views;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace NureTimetable.UI.ViewModels.Entities.ManageEntities
{
    public class ManageEntitiesViewModel : BaseViewModel
    {
        #region Properties
        private bool _isNoSourceLayoutVisible;
        public bool IsNoSourceLayoutVisible { get => _isNoSourceLayoutVisible; set => SetProperty(ref _isNoSourceLayoutVisible, value); }

        private bool _isMultiselectMode;
        public bool IsMultiselectMode
        {
            get => _isMultiselectMode;
            set => SetProperty(ref _isMultiselectMode, value, () => Entities?.ForEach(e => e.NotifyChanged(nameof(IsMultiselectMode))));
        }

        private ObservableCollection<SavedEntityItemViewModel> _entities;
        public ObservableCollection<SavedEntityItemViewModel> Entities { get => _entities; private set => SetProperty(ref _entities, value); }

        public IAsyncCommand UpdateAllCommand { get; }
        public IAsyncCommand AddEntityCommand { get; }
        public IAsyncCommand<SelectedItemChangedEventArgs> EntitySelectedCommand { get; }
        #endregion

        public ManageEntitiesViewModel()
        {
            UpdateAllCommand = CommandHelper.Create(UpdateAll, () => Entities.Any() && Entities.All(e => !e.IsUpdating));
            AddEntityCommand = CommandHelper.Create(() => Navigation.PushAsync(new AddTimetablePage()));
            EntitySelectedCommand = CommandHelper.Create<SelectedItemChangedEventArgs>(async (args) =>
            {
                if (args.SelectedItem is not SavedEntityItemViewModel entity) 
                    return;

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

            UpdateItems(UniversityEntitiesRepository.GetSaved());
            MessagingCenter.Subscribe<Application, List<SavedEntity>>(this, MessageTypes.SavedEntitiesChanged, (_, newSavedEntities) => UpdateItems(newSavedEntities));
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
                await TimetableService.UpdateAndDisplayResult(Entities?.Select(vm => (Entity)vm.SavedEntity).ToArray());
            }
        }

        private void UpdateItems(List<SavedEntity> newItems)
        {
            IsNoSourceLayoutVisible = (newItems.Count == 0);

            Entities = new(
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
            UpdateAllCommand.RaiseCanExecuteChanged();
        }
        #endregion
    }
}