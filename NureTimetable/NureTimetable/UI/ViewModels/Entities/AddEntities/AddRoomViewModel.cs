using NureTimetable.Core.Localization;
using NureTimetable.DAL;
using NureTimetable.DAL.Models.Local;
using System.Collections.Generic;
using System.Linq;

namespace NureTimetable.UI.ViewModels.Entities
{
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

        protected override IOrderedEnumerable<Room> SearchEntities(string searchQuery)
        {
            return _allEntities
                .Where(g => g.Name.ToLower().Contains(searchQuery) || g.Name.ToLower().Contains(searchQuery.Replace('и', 'і')) || g.ID.ToString() == searchQuery)
                .OrderBy(g => g.Name);
        }
    }
}