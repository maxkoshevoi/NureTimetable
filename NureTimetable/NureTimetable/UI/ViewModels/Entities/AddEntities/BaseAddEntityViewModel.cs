using NureTimetable.Core.BL;
using NureTimetable.Core.Extensions;
using NureTimetable.Core.Localization;
using NureTimetable.Core.Models.Consts;
using NureTimetable.DAL;
using NureTimetable.DAL.Models.Local;
using NureTimetable.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace NureTimetable.UI.ViewModels
{
    public abstract class BaseAddEntityViewModel<T> : BaseViewModel
    {
        private protected List<T> _allEntities;
        private string lastSearchQuery;

        #region Properties
        // ObservableRangeCollection.ReplaceRange causes ArgumentOutOfRangeException in UpdateEntities from time to time
        public ObservableCollection<T> entities = new();
        public ObservableCollection<T> Entities { get => entities; set => SetProperty(ref entities, value); }

        private protected bool _isProgressLayoutVisible;
        public bool IsProgressLayoutVisible { get => _isProgressLayoutVisible; set => SetProperty(ref _isProgressLayoutVisible, value); }

        private protected T _selectedEntity;
        public T SelectedEntity
        {
            get => _selectedEntity;
            set
            {
                if (value != null)
                    EntitySelected(value).Forget();

                _selectedEntity = value;
            }
        }

        public Command SearchBarTextChangedCommand { get; }

        #endregion

        protected BaseAddEntityViewModel()
        {
            SearchBarTextChangedCommand = CommandFactory.Create<string>(SearchBarTextChanged);

            MessagingCenter.Subscribe<Application>(this, MessageTypes.UniversityEntitiesUpdated, async _ => await UpdateEntities());
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
            bool isUpdated = await UniversityEntitiesRepository.ModifySavedAsync(savedEntities =>
            {
                if (savedEntities.Any(e => e == newEntity))
                {
                    try
                    {
                        Shell.Current.CurrentPage.DisplayToastAsync(string.Format(LN.TimetableAlreadySaved, newEntity.Entity.Name)).Forget();
                    }
                    catch { } // TODO: Remove when https://github.com/xamarin/XamarinCommunityToolkit/issues/959 is fixed
                    return true;
                }

                savedEntities.Add(newEntity);
                return false;
            });
            if (!isUpdated)
            {
                return;
            }

            try
            {
                Shell.Current.CurrentPage.DisplaySnackBarAsync(string.Format(LN.TimetableSaved, newEntity.Entity.Name), LN.Undo, 
                    () => UniversityEntitiesRepository.ModifySavedAsync(savedEntities => !savedEntities.Remove(newEntity))
                ).Forget();
            }
            catch { } // TODO: Remove when https://github.com/xamarin/XamarinCommunityToolkit/issues/959 is fixed
        }

        protected void SearchBarTextChanged(string searchQuery)
        {
            if (_allEntities == null) return;

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

        public Task UpdateEntities(Task updateDataSource = null) =>
            Task.Run(async () =>
            {
                IsProgressLayoutVisible = true;

                updateDataSource ??= Task.Run(UniversityEntitiesRepository.AssureInitialized);
                await updateDataSource;

                _allEntities = GetAllEntities();
                Entities = new(OrderEntities());

                if (!string.IsNullOrEmpty(lastSearchQuery))
                {
                    SearchBarTextChanged(lastSearchQuery);
                }

                IsProgressLayoutVisible = false;
            });

        #endregion
    }
}
