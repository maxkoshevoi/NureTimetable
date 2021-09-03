﻿using AndroidX.AppCompat.App;
using Microsoft.Maui.Controls;
using NureTimetable.Core.Models.InterplatformCommunication;
using NureTimetable.Platforms.Android.Dependences;

[assembly: Dependency(typeof(NightModeManager))]
namespace NureTimetable.Platforms.Android.Dependences
{
    public class NightModeManager : INightModeManager
    {
        public NightModeStyle DefaultNightMode
        {
            get => (NightModeStyle)AppCompatDelegate.DefaultNightMode;
            set => AppCompatDelegate.DefaultNightMode = (int)value;
        }
    }
}
