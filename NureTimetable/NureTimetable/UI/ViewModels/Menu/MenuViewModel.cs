using NureTimetable.Core.Localization;
using NureTimetable.UI.ViewModels.Core;
using NureTimetable.UI.Views;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace NureTimetable.UI.ViewModels.Menu
{
    public class MenuViewModel : BaseViewModel
    {
        private static MainPage RootPage { get => Application.Current.MainPage as MainPage; }
        private ObservableCollection<HomeMenuItem> _menuItems;
        private HomeMenuItem _selectedItem;

        public ObservableCollection<HomeMenuItem> MenuItems
        {
            get
            {
                if (_menuItems == null)
                {
                    SetProperty(ref _menuItems, new ObservableCollection<HomeMenuItem>(GetMenuItems()));
                }
                return _menuItems;
            }
        }

        public HomeMenuItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (value != null && RootPage != null)
                {
                    MainThread.BeginInvokeOnMainThread(async () => await RootPage.NavigateFromMenu((int)value.Id));
                }

                _selectedItem = value;
            }
        }

        public MenuViewModel(INavigation navigation) : base(navigation)
        {
            _selectedItem = MenuItems[0];
        }

        private static IEnumerable<HomeMenuItem> GetMenuItems()
        {
            yield return new HomeMenuItem
            {
                Id = MenuItemType.Timetable,
                Title = LN.Timetable
            };
            yield return new HomeMenuItem
            {
                Id = MenuItemType.About,
                Title = LN.About
            };
            yield return new HomeMenuItem
            {
                Id = MenuItemType.Donate,
                Title = LN.Donate
            };
        }
    }
}