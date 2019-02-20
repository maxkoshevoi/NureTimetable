using System.Collections.Generic;
using NureTimetable.Models.System;
using Xamarin.Forms;

namespace NureTimetable.ViewModels.System
{
    public class MenuViewmMdel
    {
        /// <summary>
        /// The menu items
        /// </summary>
        private IList<HomeMenuItem> _menuItems;
        /// <summary>
        /// Gets the menu items.
        /// </summary>
        /// <value>
        /// The menu items.
        /// </value>
        public IList<HomeMenuItem> MenuItems => _menuItems ?? (_menuItems = GetMenuItems());


        private IList<HomeMenuItem> GetMenuItems()
        {
            var menuItemModels = new List<HomeMenuItem>
            {
                    new HomeMenuItem {Id = MenuItemType.Timetable, Title="Расписание" },
                    new HomeMenuItem {Id = MenuItemType.About, Title="О программе" },
                    new HomeMenuItem {Id = MenuItemType.Donate, Title="Пожертвовать" }

        };

            return menuItemModels;
        }
    }
}