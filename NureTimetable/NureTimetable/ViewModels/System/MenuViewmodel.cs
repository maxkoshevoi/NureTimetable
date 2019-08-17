﻿using System.Collections.Generic;
using NureTimetable.Core.Localization;
using NureTimetable.Models.System;

namespace NureTimetable.ViewModels.System
{
    public class MenuViewModel
    {
        private IList<HomeMenuItem> _menuItems;

        public IList<HomeMenuItem> MenuItems => _menuItems ?? (_menuItems = GetMenuItems());

        private IList<HomeMenuItem> GetMenuItems()
        {
            var menuItemModels = new List<HomeMenuItem>
            {
                    new HomeMenuItem
                    {
                        Id = MenuItemType.Timetable,
                        Title = LN.Timetable
                    },
                    new HomeMenuItem
                    {
                        Id = MenuItemType.About,
                        Title = LN.About
                    },
                    new HomeMenuItem
                    {
                        Id = MenuItemType.Donate,
                        Title = LN.Donate
                    }
            };

            return menuItemModels;
        }
    }
}