using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using NureTimetable.Core.Localization;
using NureTimetable.Models.System;
using NureTimetable.UI.ViewModels.Core;
using NureTimetable.UI.Views;
using Xamarin.Forms;

namespace NureTimetable.UI.ViewModels.System
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
                if (value != null)
                {
                    Device.BeginInvokeOnMainThread(async () => { await ItemSelected(value); });
                }

                _selectedItem = value;
            }
        }

        public MenuViewModel(INavigation navigation) : base(navigation)
        {
            SelectedItem = MenuItems[0];
        }

        private static async Task ItemSelected(HomeMenuItem selectedItem)
        {
            if (selectedItem == null)
            {
                return;
            }

            await RootPage.NavigateFromMenu((int)selectedItem.Id);
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