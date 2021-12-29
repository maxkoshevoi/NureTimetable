using System.Linq;
using Xamarin.CommunityToolkit.ObjectModel;

namespace NureTimetable.UI.ViewModels.Lessons.LessonSettings
{
    public class ListViewViewModel<T> : BaseViewModel
    {
        public ObservableRangeCollection<CheckedEntity<T>> ItemsSource { get; } = new();
        
        private bool _isVisible;
        public bool IsVisible { get => _isVisible; set => SetProperty(ref _isVisible, value); }
        
        public ListViewViewModel()
        {
            ItemsSource.CollectionChanged += (_, _) =>
            {
                IsVisible = ItemsSource.Any();
            };
        }
    }
}
