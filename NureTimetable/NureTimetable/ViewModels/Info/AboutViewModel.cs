﻿using NureTimetable.Core.Models.InterplatformCommunication;
using NureTimetable.Services.Helpers;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace NureTimetable.ViewModels.Info
{
    public class AboutViewModel
    {
        public string VersionText { get; set; }

        public bool IsDebugModeActive
        {
            get => App.IsDebugMode;
            set
            {
                App.IsDebugMode = value;
            }
        }

        public ICommand NavigateUriCommand { get; }

        public AboutViewModel()
        {
            NavigateUriCommand = CommandHelper.CreateCommand<string>(NavigateUri);
            var version = DependencyService.Get<IAppVersionProvider>();
            VersionText = version?.AppVersion ?? "1.0";
        }

        private async Task NavigateUri(string url)
        {
            Device.OpenUri(new Uri(url));
        }
    }
}