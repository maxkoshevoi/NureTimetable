using NureTimetable.Core.Localization;
using NureTimetable.Core.Models.Consts;
using NureTimetable.DAL;
using NureTimetable.DAL.Models.Local;
using NureTimetable.Services.Helpers;
using NureTimetable.UI.ViewModels.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace NureTimetable.UI.ViewModels.TimetableEntities
{
    public abstract class BaseAddEntityViewModel<T> : BaseViewModel
    {
        #region variables

        private protected List<T> _allEntities;

        private protected ObservableCollection<T> _entities;

        private protected bool _progressLayoutIsVisable;

        private protected bool _progressLayoutIsEnable;

        private protected bool _noSourceLayoutIsVisible;

        private protected string _searchBarText;

        private protected T _selectedEntity;

        #endregion

        #region Abstract Properties

        public abstract string Title { get; }

        #endregion

        #region Properties

        public ObservableCollection<T> Entities { get => _entities; private protected set => SetProperty(ref _entities, value); }

        public bool ProgressLayoutIsVisable
        {
            get => _progressLayoutIsVisable;
            set => SetProperty(ref _progressLayoutIsVisable, value);
        }

        public bool ProgressLayoutIsEnable
        {
            get => _progressLayoutIsEnable;
            set => SetProperty(ref _progressLayoutIsEnable, value);
        }

        public bool NoSourceLayoutIsVisible
        {
            get => _noSourceLayoutIsVisible;
            set => SetProperty(ref _noSourceLayoutIsVisible, value);
        }

        public string SearchBarText { get => _searchBarText; set => SetProperty(ref _searchBarText, value); }

        public ICommand UpdateCommand { get; protected set; }

        public ICommand SearchBarTextChangedCommand { get; protected set; }

        public ICommand ContentPageAppearingCommand { get; protected set; }

        #endregion

        public BaseAddEntityViewModel(INavigation navigation) : base(navigation)
        {
            SearchBarTextChangedCommand = CommandHelper.CreateCommand(SearchBarTextChanged);
            ContentPageAppearingCommand = CommandHelper.CreateCommand(UpdateEntities);
            UpdateCommand = CommandHelper.CreateCommand(UpdateFromCist);
            Device.BeginInvokeOnMainThread(async () => await UpdateEntities(false));

            MessagingCenter.Subscribe<Application>(this, MessageTypes.UniversityEntitiesUpdated, async (sender) =>
            {
                await UpdateEntities();
            });
        }

        #region Abstract Methods

        protected abstract IOrderedEnumerable<T> OrderEntities();

        protected abstract IOrderedEnumerable<T> SearchEntities(string searchQuery);

        protected abstract List<T> GetAllEntitiesFromCist();

        #endregion

        #region Methods

        public T SelectedEntity
        {
            get => _selectedEntity;
            set
            {
                if (value != null)
                {
                    Device.BeginInvokeOnMainThread(async () => { await EntitySelected(value); });
                }

                _selectedEntity = value;
            }
        }

        protected abstract SavedEntity GetSavedEntity(T entity);

        protected async Task EntitySelected(T entity)
        {
            SavedEntity newEntity = GetSavedEntity(entity);

            List<SavedEntity> savedEntities = UniversityEntitiesRepository.GetSaved();
            if (savedEntities.Exists(e => e == newEntity))
            {
                await App.Current.MainPage.DisplayAlert(LN.AddingTimetable, string.Format(LN.TimetableAlreadySaved, newEntity.Name), LN.Ok);
                return;
            }

            savedEntities.Add(newEntity);
            UniversityEntitiesRepository.UpdateSaved(savedEntities);

            await App.Current.MainPage.DisplayAlert(LN.AddingTimetable, string.Format(LN.TimetableSaved, newEntity.Name), LN.Ok);
        }

        protected void SearchBarTextChanged()
        {
            if (_allEntities == null) return;

            if (string.IsNullOrEmpty(SearchBarText))
            {
                Entities = new ObservableCollection<T>(OrderEntities());
            }
            else
            {
                string searchQuery = SearchBarText.ToLower();
                Entities =
                    new ObservableCollection<T>(
                        SearchEntities(searchQuery)
                    );
            }
        }

        protected async Task UpdateFromCist()
        {
            if (SettingsRepository.CheckCistAllEntitiesUpdateRights() == false)
            {
                await App.Current.MainPage.DisplayAlert(LN.UniversityInfoUpdate, LN.UniversityInfoUpToDate, LN.Ok);
                return;
            }

            if (!ProgressLayoutIsVisable && await App.Current.MainPage.DisplayAlert(LN.UniversityInfoUpdate, LN.UniversityInfoUpdateConfirm, LN.Yes, LN.Cancel))
            {
                await UpdateEntities(true);
            }
        }

        protected async Task UpdateEntities()
        {
            await UpdateEntities(false);
        }

        protected async Task UpdateEntities(bool fromCistOnly = false)
        {
            ProgressLayoutIsVisable = true;
            ProgressLayoutIsEnable = false;

            await Task.Factory.StartNew(() =>
            {
                UniversityEntitiesRepository.UniversityEntitiesCistUpdateResult updateFromCistResult = null;
                if (fromCistOnly)
                {
                    updateFromCistResult = UniversityEntitiesRepository.UpdateFromCist();

                    if (updateFromCistResult.IsAllFail)
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            App.Current.MainPage.DisplayAlert(LN.UniversityInfoUpdate, LN.UniversityInfoUpdateFail, LN.Ok);
                        });

                        ProgressLayoutIsVisable = false;
                        ProgressLayoutIsEnable = true;

                        return;
                    }
                    else if (!updateFromCistResult.IsAllSuccessful)
                    {
                        string failedEntities = Environment.NewLine;
                        if (!updateFromCistResult.IsGroupsOk)
                        {
                            failedEntities += LN.Groups + Environment.NewLine;
                        }
                        if (!updateFromCistResult.IsTeachersOk)
                        {
                            failedEntities += LN.Teachers + Environment.NewLine;
                        }
                        if (!updateFromCistResult.IsRoomsOk)
                        {
                            failedEntities += LN.Rooms + Environment.NewLine;
                        }

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            App.Current.MainPage.DisplayAlert(LN.UniversityInfoUpdate, string.Format(LN.UniversityInfoUpdatePartiallyFail, Environment.NewLine + failedEntities), LN.Ok);
                        });
                    }
                }
                else
                {
                    UniversityEntitiesRepository.AssureInitialized();
                }
                _allEntities = GetAllEntitiesFromCist();
                Entities = new ObservableCollection<T>(OrderEntities());
                
                NoSourceLayoutIsVisible = Entities.Count == 0;

                if (SearchBarTextChangedCommand.CanExecute(null))
                {
                    SearchBarTextChangedCommand.Execute(null);
                }

                ProgressLayoutIsVisable = false;
                ProgressLayoutIsEnable = true;
            });
        }

        #endregion
    }
}
