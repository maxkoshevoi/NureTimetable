using Microsoft.Maui.Controls;
using NureTimetable.BL;
using NureTimetable.Core.Localization;
using NureTimetable.Core.Models.Consts;
using NureTimetable.DAL;
using NureTimetable.DAL.Models.Local;
using NureTimetable.UI.Models.Consts;
using NureTimetable.UI.Views;
using System.ComponentModel;
using Xamarin.CommunityToolkit.ObjectModel;

namespace NureTimetable.UI.ViewModels
{
    public class ManageEntitiesViewModel : BaseViewModel
    {
        private readonly object updatingEntities = new();

        #region Properties
        private bool _isMultiselectMode;
        public bool IsMultiselectMode { get => _isMultiselectMode; set => SetProperty(ref _isMultiselectMode, value); }

        private bool _isProgressLayoutVisible;
        public bool IsProgressLayoutVisible { get => _isProgressLayoutVisible; set => SetProperty(ref _isProgressLayoutVisible, value); }

        public ObservableRangeCollection<SavedEntityItemViewModel> Entities { get; } = new();

        public IAsyncCommand PageAppearingCommand { get; }
        public IAsyncCommand UpdateAllCommand { get; }
        public IAsyncCommand AddEntityCommand { get; }
        public IAsyncCommand<SelectedItemChangedEventArgs> EntitySelectedCommand { get; }
        #endregion

        public ManageEntitiesViewModel()
        {
            PageAppearingCommand = CommandFactory.Create(PageAppearing);
            UpdateAllCommand = CommandFactory.Create(UpdateAll, () => { lock (updatingEntities) { return Entities.Any() && Entities.All(e => !e.IsUpdating); } }, allowsMultipleExecutions: false);
            AddEntityCommand = CommandFactory.Create(() => Navigation.PushAsync(new AddTimetablePage()), allowsMultipleExecutions: false);
            EntitySelectedCommand = CommandFactory.Create<SelectedItemChangedEventArgs>(async args =>
            {
                if (args!.SelectedItem is not SavedEntityItemViewModel entity)
                    return;

                SavedEntity savedEntity = entity.SavedEntity;
                if (IsMultiselectMode)
                {
                    savedEntity.IsSelected = !savedEntity.IsSelected;
                }
                else
                {
                    await SelectOne(savedEntity);
                    await Shell.Current.GoToAsync(Route.EventsTab);
                }
            });

            MessagingCenter.Subscribe<Application, List<SavedEntity>>(this, MessageTypes.SavedEntitiesChanged, (_, newSavedEntities) => UpdateItems(newSavedEntities));
            MessagingCenter.Subscribe<Application, Entity>(this, MessageTypes.TimetableUpdating, (sender, entity) =>
            {
                lock (updatingEntities)
                {
                    SavedEntityItemViewModel? savedEntity = Entities.SingleOrDefault(e => e.SavedEntity == entity);
                    if (savedEntity != null)
                        savedEntity.IsUpdating = true;
                }
            });
            MessagingCenter.Subscribe<Application, Entity>(this, MessageTypes.TimetableUpdated, (sender, entity) =>
            {
                lock (updatingEntities)
                {
                    SavedEntityItemViewModel? savedEntity = Entities.SingleOrDefault(e => e.SavedEntity == entity);
                    if (savedEntity != null)
                        savedEntity.IsUpdating = false;
                }
            });

            // ListIsNullOrEmptyConverter needs to know that Entities are updated
            Entities.CollectionChanged += (_, _) =>
            {
                UpdateAllCommand.RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(Entities));
            };

            IsProgressLayoutVisible = true;
        }

        public async Task PageAppearing()
        {
            if (Entities.None())
            {
                UpdateItems(await UniversityEntitiesRepository.GetSavedAsync());
                IsProgressLayoutVisible = false;
            }
        }

        public Task SelectOne(SavedEntity savedEntity) => UniversityEntitiesRepository.ModifySavedAsync(savedEntities =>
        {
            foreach (var e in savedEntities)
            {
                e.IsSelected = e == savedEntity;
            }
        });

        public async void EntityChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(SavedEntity.IsSelected))
                return;

            await EntitySelectChanged((SavedEntity)sender!);
        }

        public Task EntitySelectChanged(SavedEntity entity) =>
            UniversityEntitiesRepository.ModifySavedAsync(currentSaved =>
            {
                SavedEntity savedEntity = currentSaved.Single(e => e == entity);

                // Check state is changed
                if (savedEntity.IsSelected == entity.IsSelected)
                {
                    return;
                }
                savedEntity.IsSelected = entity.IsSelected;

                // User cannot deselect last selected entity
                if (!savedEntity.IsSelected && currentSaved.None(e => e.IsSelected) && Entities.Any(e => e.SavedEntity == entity))
                {
                    savedEntity.IsSelected = true;
                    return;
                }
            });

        private async Task UpdateAll()
        {
            if (await Shell.Current.DisplayAlert(LN.TimetableUpdate, LN.UpdateAllTimetables, LN.Yes, LN.Cancel))
            {
                await TimetableService.UpdateAndDisplayResultAsync(Entities.Select(vm => (Entity)vm.SavedEntity).ToArray());
            }
        }

        private void UpdateItems(List<SavedEntity> newItems)
        {
            lock (updatingEntities)
            {
                Entities.ToList().ForEach(se => se.SavedEntity.PropertyChanged -= EntityChanged);
                Entities.ReplaceRange(newItems.Select(se => new SavedEntityItemViewModel(se, this)).ToArray());
                Entities.ToList().ForEach(se => se.SavedEntity.PropertyChanged += EntityChanged);

                IsMultiselectMode = Entities.Count(i => i.SavedEntity.IsSelected) > 1;
            }
        }
    }
}