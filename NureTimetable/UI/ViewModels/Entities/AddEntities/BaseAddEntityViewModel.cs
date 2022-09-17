using System.Collections.ObjectModel;

namespace NureTimetable.UI.ViewModels;

public abstract partial class BaseAddEntityViewModel<T> : BaseViewModel
{
    private protected List<T> _allEntities = new();

    #region Properties
    // ObservableRangeCollection.ReplaceRange causes ArgumentOutOfRangeException in UpdateEntities from time to time
    [ObservableProperty] public ObservableCollection<T> entities = new();
    [ObservableProperty] private protected bool _isProgressLayoutVisible;
    [ObservableProperty] private string lastSearchQuery = string.Empty;

    public T? SelectedEntity
    {
        get => default;
        set
        {
            if (value != null)
            {
                EntitySelected(value).Forget();
            }
        }
    }
    #endregion

    protected BaseAddEntityViewModel()
    {
        MessagingCenter.Subscribe<Application>(this, MessageTypes.UniversityEntitiesUpdated, async _ => await UpdateEntities());
    }

    #region Abstract Methods

    protected abstract IOrderedEnumerable<T> OrderEntities();

    protected abstract IOrderedEnumerable<T> SearchEntities(string query);

    protected IOrderedEnumerable<T> SearchEntities(string query, Func<T, string> nameSelector, Func<T, long> idSelector)
    {
        query = query.Simplify();

        return _allEntities
            .Where(e => nameSelector(e).Simplify().Contains(query) || idSelector(e).ToString() == query)
            .OrderBy(e => nameSelector(e));
    }

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
                Toast.Make(string.Format(LN.TimetableAlreadySaved, newEntity.Entity.Name)).Show().Forget();
                return true;
            }

            savedEntities.Add(newEntity);
            return false;
        });
        if (!isUpdated)
        {
            return;
        }

        Snackbar.Make(
            string.Format(LN.TimetableSaved, newEntity.Entity.Name), 
            async () => await UniversityEntitiesRepository.ModifySavedAsync(savedEntities => !savedEntities.Remove(newEntity)), 
            LN.Undo)
            .Show().Forget();
    }

    public void SearchQueryChanged(string searchQuery)
    {
        LastSearchQuery = searchQuery;
        if (string.IsNullOrEmpty(searchQuery))
        {
            Entities = new(OrderEntities());
        }
        else
        {
            Entities = new(SearchEntities(searchQuery));
        }
    }

    public Task UpdateEntities(Task? updateDataSource = null) =>
        Task.Run(async () =>
        {
            IsProgressLayoutVisible = true;

            updateDataSource ??= Task.Run(UniversityEntitiesRepository.AssureInitialized);
            await updateDataSource;

            _allEntities = GetAllEntities();
            SearchQueryChanged(LastSearchQuery);

            IsProgressLayoutVisible = false;
        });

    #endregion
}
