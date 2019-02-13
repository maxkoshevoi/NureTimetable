﻿using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using NureTimetable.Views;
using Syncfusion.Licensing;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace NureTimetable
{
    public partial class App : Application
    {
        public static bool IsDebugMode = false;

        public App()
        {
            //Register Syncfusion license
            SyncfusionLicenseProvider.RegisterLicense("Njc3NDVAMzEzNjJlMzQyZTMwYUJReFEwbjdlU1BNSndTYTcramc0cmdxL2dDdEFVT0syOU5xa2hlLzdhOD0=");

            InitializeComponent();
            
            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
            AppCenter.Start("android=54a8f346-64c8-44f9-bbd5-6a0dae141d93;", typeof(Analytics), typeof(Crashes));
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
