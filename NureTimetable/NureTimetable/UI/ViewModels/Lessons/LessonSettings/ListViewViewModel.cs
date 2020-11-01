using System.Collections.ObjectModel;
using System.Linq;

namespace NureTimetable.UI.ViewModels.Lessons.LessonSettings
{
    public class ListViewViewModel<T> : BaseViewModel
    {
        private ObservableCollection<CheckedEntity<T>> _itemSource;
        private bool _isVisible;
        private double _heightRequest;

        public ObservableCollection<CheckedEntity<T>> ItemsSource { get => _itemSource; internal set => SetProperty(ref _itemSource, value, onChanged: ItemsSourceChanged); }
        public bool IsVisible { get => _isVisible; set => SetProperty(ref _isVisible, value); }
        public double HeightRequest { get => _heightRequest; set => SetProperty(ref _heightRequest, value); }

        private void ItemsSourceChanged()
        {
            if (ItemsSource is null || !ItemsSource.Any())
            {
                IsVisible = false;
            }
            else
            {
                Resize();
                IsVisible = true;
            }
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
