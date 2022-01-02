using NureTimetable.Core.Localization;
using NureTimetable.DAL.Cist;
using NureTimetable.DAL.Models;

namespace NureTimetable.UI.ViewModels;

public class AddRoomViewModel : BaseAddEntityViewModel<Room>
{
    public AddRoomViewModel()
    {
        Title = new(() => LN.Rooms);
    }

    protected override List<Room> GetAllEntities()
    {
        return UniversityEntitiesRepository.GetAllRooms().ToList();
    }

    protected override SavedEntity GetSavedEntity(Room entity)
    {
        return new SavedEntity(new Entity(entity));
    }

    protected override IOrderedEnumerable<Room> OrderEntities()
    {
        return _allEntities.OrderBy(r => r.Name);
    }

    protected override IOrderedEnumerable<Room> SearchEntities(string query) =>
        SearchEntities(query, r => r.Name, r => r.ID);
}
