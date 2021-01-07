using NureTimetable.Core.Localization;
using NureTimetable.Core.Models.Consts;
using NureTimetable.DAL;
using NureTimetable.DAL.Models.Local;
using NureTimetable.UI.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace NureTimetable.UI.ViewModels.Entities
{
    public abstract class BaseAddEntityViewModel<T> : BaseViewModel
    {
        private protected List<T> _allEntities;
        private string lastSearchQuery;

        #region Properties
        private protected ObservableCollection<T> _entities;
        public ObservableCollection<T> Entities { get => _entities; private protected set => SetProperty(ref _entities, value); }

        private protected bool _isProgressLayoutVisible;
        public bool IsProgressLayoutVisible { get => _isProgressLayoutVisible; set => SetProperty(ref _isProgressLayoutVisible, value); }

        private protected bool _isNoSourceLayoutVisible;
        public bool IsNoSourceLayoutVisible { get => _isNoSourceLayoutVisible; set => SetProperty(ref _isNoSourceLayoutVisible, value); }

        private protected T _selectedEntity;
        public T SelectedEntity
        {
            get => _selectedEntity;
            set
            {
                if (value != null)
                    MainThread.BeginInvokeOnMainThread(async () => await EntitySelected(value));

                _selectedEntity = value;
            }
        }

        public Command SearchBarTextChangedCommand { get; }

        #endregion

        protected BaseAddEntityViewModel()
        {
            SearchBarTextChangedCommand = CommandHelper.Create<string>(SearchBarTextChanged);

            MessagingCenter.Subscribe<Application>(this, MessageTypes.UniversityEntitiesUpdated, async (sender) => await UpdateEntities());
            MainThread.BeginInvokeOnMainThread(async () => await UpdateEntities());
        }

        #region Abstract Methods

        protected abstract IOrderedEnumerable<T> OrderEntities();

        protected abstract IOrderedEnumerable<T> SearchEntities(string searchQuery);

        protected abstract List<T> GetAllEntities();

        #endregion

        #region Methods

        protected abstract SavedEntity GetSavedEntity(T entity);

        protected async Task EntitySelected(T entity)
        {
            SavedEntity newEntity = GetSavedEntity(entity);

            List<SavedEntity> savedEntities = UniversityEntitiesRepository.GetSaved();
            if (savedEntities.Any(e => e == newEntity))
            {
                await Shell.Current.DisplayAlert(LN.AddingTimetable, string.Format(LN.TimetableAlreadySaved, newEntity.Name), LN.Ok);
                return;
            }

            savedEntities.Add(newEntity);
            UniversityEntitiesRepository.UpdateSaved(savedEntities);

            await Shell.Current.DisplayAlert(LN.AddingTimetable, string.Format(LN.TimetableSaved, newEntity.Name), LN.Ok);
        }

        protected void SearchBarTextChanged(string searchQuery)
        {
            if (_allEntities is null) return;

            lastSearchQuery = searchQuery;
            if (string.IsNullOrEmpty(searchQuery))
            {
                Entities = new(OrderEntities());
            }
            else
            {
                Entities = new(SearchEntities(searchQuery.ToLower()));
            }
        }

        public async Task UpdateEntities(Task updateDataSource = null)
        {
            await Task.Run(async () =>
            {
                updateDataSource ??= Task.Run(UniversityEntitiesRepository.AssureInitialized);

                IsProgressLayoutVisible = true;

                await updateDataSource;
                _allEntities = GetAllEntities();
                Entities = new(OrderEntities());

                IsNoSourceLayoutVisible = Entities.Count == 0;

                if (SearchBarTextChangedCommand.CanExecute(lastSearchQuery))
                {
                    SearchBarTextChangedCommand.Execute(lastSearchQuery);
                }

                IsProgressLayoutVisible = false;
            });
        }

        #endregion
    }
}
