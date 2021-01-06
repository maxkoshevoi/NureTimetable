using System;

namespace NureTimetable.UI.ViewModels.Lessons.LessonSettings
{
    public class CheckedEntity<T> : BaseViewModel
    {
        private readonly Action _stateChanged;

        public T Entity { get; }

        private bool? _isChecked;
        public bool? IsChecked { get => _isChecked; set => SetProperty(ref _isChecked, value, _stateChanged); }

        public CheckedEntity(T entity, Action<CheckedEntity<T>> stateChanged = null)
        {
            Entity = entity;
            _stateChanged = () => stateChanged?.Invoke(this);
        }
    }
}
