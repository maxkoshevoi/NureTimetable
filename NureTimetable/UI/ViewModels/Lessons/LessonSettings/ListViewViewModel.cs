using System.Linq;
using Xamarin.CommunityToolkit.ObjectModel;

namespace NureTimetable.UI.ViewModels.Lessons.LessonSettings
{
    public class ListViewViewModel<T> : BaseViewModel
    {
        public ObservableRangeCollection<CheckedEntity<T>> ItemsSource { get; } = new();
        
        private bool _isVisible;
        public bool IsVisible { get => _isVisible; set => SetProperty(ref _isVisible, value); }
        
        private double _heightRequest;
        public double HeightRequest { get => _heightRequest; set => SetProperty(ref _heightRequest, value); }

        public ListViewViewModel()
        {
            ItemsSource.CollectionChanged += (_, _) =>
            {
                IsVisible = ItemsSource.Any();
                Resize();
            };
        }

        private void Resize()
        {
            // ListView doesn't support AutoSize, so we have to do it manually
            const int rowHeight = 50,
                headerHeight = 19;

            HeightRequest = rowHeight * ItemsSource.Count + headerHeight;
        }
    }
}
