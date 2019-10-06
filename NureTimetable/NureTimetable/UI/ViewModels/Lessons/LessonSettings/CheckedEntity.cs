using NureTimetable.UI.ViewModels.Core;
using System;
using Xamarin.Forms;

namespace NureTimetable.UI.ViewModels.Lessons.LessonSettings
{
    public class CheckedEntity<T> : BaseViewModel
    {
        private T _entity;
        private bool? _isChecked;
        private readonly Action _stateChanged;

        public T Entity { get => _entity; set => SetProperty(ref _entity, value); }
        public bool? IsChecked { get => _isChecked; set => SetProperty(ref _isChecked, value, onChanged: _stateChanged); }

        public CheckedEntity(INavigation navigation, Action<CheckedEntity<T>> stateChanged = null) : base(navigation)
        {
            _stateChanged = () => stateChanged?.Invoke(this);
        }
    }
}
