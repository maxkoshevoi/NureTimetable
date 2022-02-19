namespace NureTimetable.UI.ViewModels;

public class AddGroupViewModel : BaseAddEntityViewModel<Group>
{
    public AddGroupViewModel()
    {
        Title = new(() => LN.Groups);
    }

    protected override List<Group> GetAllEntities()
    {
        return UniversityEntitiesRepository.GetAllGroups().ToList();
    }

    protected override SavedEntity GetSavedEntity(Group entity)
    {
        return new SavedEntity(new Entity(entity));
    }

    protected override IOrderedEnumerable<Group> OrderEntities()
    {
        return _allEntities.OrderBy(g => g.Name);
    }

    protected override IOrderedEnumerable<Group> SearchEntities(string query) =>
        SearchEntities(query, g => g.Name, g => g.ID);
}
