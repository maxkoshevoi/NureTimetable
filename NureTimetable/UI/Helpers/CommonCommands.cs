﻿using Microsoft.Maui.Essentials;
using System;
using Xamarin.CommunityToolkit.ObjectModel;

namespace NureTimetable.UI.Helpers
{
    public static class CommonCommands
    {
        public static IAsyncCommand<string> NavigateUriCommand { get; } =
            CommandFactory.Create<string>(async url => await Launcher.OpenAsync(new Uri(url)));
    }
}