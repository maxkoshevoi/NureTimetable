using NureTimetable.DAL;
using NureTimetable.DAL.Models.Local;
using System.Collections.Generic;
using NureTimetable.Core.Localization;
using System.Linq;
using Xamarin.Forms;

namespace NureTimetable.UI.ViewModels.TimetableEntities
{
    public class AddGroupViewModel : BaseAddEntityViewModel<Group>
    {
        public AddGroupViewModel(INavigation navigation) : base(navigation)
        {
        }

        public override string Title { get; } = LN.Groups;

        protected override List<Group> GetAllEntitiesFromCist()
        {
            return UniversityEntitiesRepository.GetAllGroups().ToList();
        }

        protected override SavedEntity GetSavedEntity(Group entity)
        {
            return new SavedEntity(entity);
        }

        protected override IOrderedEnumerable<Group> OrderEntities()
        {
            return _allEntities.OrderBy(g => g.Name);
        }

        protected override IOrderedEnumerable<Group> SearchEntities(string searchQuery)
        {
            return _allEntities
                .Where(g => g.Name.ToLower().Contains(searchQuery) || g.Name.ToLower().Contains(searchQuery.Replace('и', 'і')) || g.ID.ToString() == searchQuery)
                .OrderBy(g => g.Name);
        }
    }
}