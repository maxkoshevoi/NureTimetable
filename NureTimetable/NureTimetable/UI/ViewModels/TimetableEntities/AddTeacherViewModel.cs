using NureTimetable.DAL;
using NureTimetable.DAL.Models.Local;
using System.Collections.Generic;
using NureTimetable.Core.Localization;
using System.Linq;
using Xamarin.Forms;

namespace NureTimetable.UI.ViewModels.TimetableEntities
{
    public class AddTeacherViewModel : BaseAddEntityViewModel<Teacher>
    {
        public AddTeacherViewModel(INavigation navigation) : base(navigation)
        {
        }

        public override string Title { get; } = LN.Teachers;

        protected override List<Teacher> GetAllEntitiesFromCist()
        {
            return UniversityEntitiesRepository.GetAllTeachers().ToList();
        }

        protected override SavedEntity GetSavedEntity(Teacher entity)
        {
            return new SavedEntity(entity);
        }

        protected override IOrderedEnumerable<Teacher> OrderEntities()
        {
            return _allEntities.OrderBy(t => t.Name);
        }

        protected override IOrderedEnumerable<Teacher> SearchEntities(string searchQuery)
        {
            return _allEntities
                .Where(g => g.Name.ToLower().Contains(searchQuery) || g.Name.ToLower().Contains(searchQuery.Replace('и', 'і')) || g.ID.ToString() == searchQuery)
                .OrderBy(g => g.Name);
        }
    }
}