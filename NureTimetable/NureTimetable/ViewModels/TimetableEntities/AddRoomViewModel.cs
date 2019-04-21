using NureTimetable.DAL;
using NureTimetable.DAL.Models.Local;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace NureTimetable.ViewModels.TimetableEntities
{
    public class AddRoomViewModel : BaseAddEntityViewModel<Room>
    {
        public AddRoomViewModel(INavigation navigation) : base(navigation)
        {
        }

        public override string Title { get; } = "Аудитории";

        protected override List<Room> GetAllEntitiesFromCist()
        {
            return UniversityEntitiesRepository.GetAllRooms().ToList();
        }

        protected override SavedEntity GetSavedEntity(Room entity)
        {
            return new SavedEntity(entity);
        }

        protected override IOrderedEnumerable<Room> OrderEntities()
        {
            return _allEntities.OrderBy(g => g.Name);
        }

        protected override IOrderedEnumerable<Room> SearchEntities(string searchQuery)
        {
            return _allEntities
                .Where(g => g.Name.ToLower().Contains(searchQuery) || g.Name.ToLower().Contains(searchQuery.Replace('и', 'і')) || g.ID.ToString() == searchQuery)
                .OrderBy(g => g.Name);
        }
    }
}