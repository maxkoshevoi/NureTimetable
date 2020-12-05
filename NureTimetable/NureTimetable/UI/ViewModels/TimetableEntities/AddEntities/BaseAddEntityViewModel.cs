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

namespace NureTimetable.UI.ViewModels.TimetableEntities
{
    public abstract class BaseAddEntityViewModel<T> : BaseViewModel
    {
        private protected List<T> _allEntities;
        private string lastSearchQuery;

        #region Properties
        private protected ObservableCollection<T> _entities;
        public ObservableCollection<T> Entities { get => _entities; private protected set => SetProperty(ref _entities, value); }

        private protected bool _progressLayoutIsVisable;
        public bool ProgressLayoutIsVisable
        {
            get => _progressLayoutIsVisable;
            set => SetProperty(ref _progressLayoutIsVisable, value);
        }

        private protected bool _progressLayoutIsEnable;
        public bool ProgressLayoutIsEnable
        {
            get => _progressLayoutIsEnable;
            set => SetProperty(ref _progressLayoutIsEnable, value);
        }

        private protected bool _noSourceLayoutIsVisible;
        public bool NoSourceLayoutIsVisible
        {
            get => _noSourceLayoutIsVisible;
            set => SetProperty(ref _noSourceLayoutIsVisible, value);
        }

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

        public ICommand SearchBarTextChangedCommand { get; }

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
                Entities = new ObservableCollection<T>(OrderEntities());
            }
            else
            {
                Entities = new ObservableCollection<T>(SearchEntities(searchQuery.ToLower()));
            }
        }

        public async Task UpdateEntities(Task updateDataSource = null)
        {
            await Task.Run(async () =>
            {
                updateDataSource ??= Task.Run(UniversityEntitiesRepository.AssureInitialized);

                ProgressLayoutIsVisable = true;
                ProgressLayoutIsEnable = false;

                await updateDataSource;
                _allEntities = GetAllEntities();
                Entities = new ObservableCollection<T>(OrderEntities());

                NoSourceLayoutIsVisible = Entities.Count == 0;

                if (SearchBarTextChangedCommand.CanExecute(lastSearchQuery))
                {
                    SearchBarTextChangedCommand.Execute(lastSearchQuery);
                }

                ProgressLayoutIsVisable = false;
                ProgressLayoutIsEnable = true;
            });
        }

        #endregion
    }
}
