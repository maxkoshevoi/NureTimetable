using NureTimetable.DAL;
using NureTimetable.DAL.Models.Local;
using NureTimetable.Services.Helpers;
using NureTimetable.ViewModels.Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace NureTimetable.ViewModels.TimetableEntities
{
    public abstract class BaseAddEntityViewModel<T> : BaseViewModel
    {
        #region variables

        protected List<T> _allEntities;

        protected ObservableCollection<T> _entities;

        protected bool _progressLayoutIsVisable;

        protected bool _progressLayoutIsEnable;

        protected string _searchBarText;

        protected T _selectedEntity;

        #endregion

        #region Abstract Properties

        public abstract string Title { get; }

        #endregion

        #region Properties

        public ObservableCollection<T> Entities { get => _entities; set => SetProperty(ref _entities, value); }

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
                await App.Current.MainPage.DisplayAlert("Добавление расписания", $"Расписание \"{newEntity.Name}\" уже находится в сохранённых", "OK");
                return;
            }

            savedEntities.Add(newEntity);
            UniversityEntitiesRepository.UpdateSaved(savedEntities);

            await App.Current.MainPage.DisplayAlert("Добавление расписания", $"Расписание \"{newEntity.Name}\" добавлено в сохранённые", "OK");
        }

        protected async Task SearchBarTextChanged()
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
                await App.Current.MainPage.DisplayAlert("Обновление информации об университете", "У вас уже загружена последняя версия информации об университете", "Ok");
                return;
            }

            if (!ProgressLayoutIsVisable && await App.Current.MainPage.DisplayAlert("Обновление информации об университете",
                    "Вы уверенны, что хотите обновить информациию об университете из Cist?", "Да", "Отмена"))
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
                if (fromCistOnly)
                {
                    UniversityEntitiesRepository.UpdateFromCist();
                }
                else
                {
                    UniversityEntitiesRepository.Init();
                }
                _allEntities = GetAllEntitiesFromCist();

                if (_allEntities == null)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        App.Current.MainPage.DisplayAlert("Обновление информации об университете", "Не удалось обновить информациию об университете. Пожалуйста, попробуйте позже.", "Ok");
                    });

                    ProgressLayoutIsVisable = false;
                    ProgressLayoutIsEnable = true;

                    return;
                }

                Entities = new ObservableCollection<T>(OrderEntities());

                if (SearchBarTextChangedCommand.CanExecute(null))
                    SearchBarTextChangedCommand.Execute(null);

                ProgressLayoutIsVisable = false;
                ProgressLayoutIsEnable = true;
            });
        }

        #endregion
    }
}
