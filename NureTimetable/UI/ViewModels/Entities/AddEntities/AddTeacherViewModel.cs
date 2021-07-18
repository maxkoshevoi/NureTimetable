using NureTimetable.Core.Localization;
using NureTimetable.DAL;
using NureTimetable.DAL.Models.Local;
using System.Collections.Generic;
using System.Linq;

namespace NureTimetable.UI.ViewModels
{
    public class AddTeacherViewModel : BaseAddEntityViewModel<Teacher>
    {
        public AddTeacherViewModel()
        {
            Title = new(() => LN.Teachers);
        }

        protected override List<Teacher> GetAllEntities()
        {
            return UniversityEntitiesRepository.GetAllTeachers().ToList();
        }

        protected override SavedEntity GetSavedEntity(Teacher entity)
        {
            return new SavedEntity(new Entity(entity));
        }

        protected override IOrderedEnumerable<Teacher> OrderEntities()
        {
            return _allEntities.OrderBy(t => t.Name);
        }

        protected override IOrderedEnumerable<Teacher> SearchEntities(string query) =>
            SearchEntities(query, t => t.Name, t => t.ID);
    }
}