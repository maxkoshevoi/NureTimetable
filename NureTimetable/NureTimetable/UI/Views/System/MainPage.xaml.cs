using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using NureTimetable.Core.Localization;
using NureTimetable.Core.Models.Consts;
using NureTimetable.Migrations;
using NureTimetable.UI.ViewModels.Info;
using NureTimetable.UI.ViewModels.Menu;
using NureTimetable.UI.ViewModels.Timetable;
using NureTimetable.UI.Views.Info;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using NureTimetable.Core.Extensions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NureTimetable.UI.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : MasterDetailPage
    {
        readonly Dictionary<int, NavigationPage> MenuPages = new Dictionary<int, NavigationPage>();

        public MainPage()
        {
            InitializeComponent();

            Master = new MenuPage
            {
                BindingContext = new MenuViewModel(Navigation)
            };
            var timetablePage = new TimetablePage();
            timetablePage.BindingContext = new TimetableViewModel(timetablePage.Navigation, timetablePage);
            Detail = new NavigationPage(timetablePage);

            MasterBehavior = MasterBehavior.Popover;
            MenuPages.Add((int)MenuItemType.Timetable, (NavigationPage)Detail);

            MessagingCenter.Subscribe<Application, Exception>(this, MessageTypes.ExceptionOccurred, (sender, ex) =>
            {
                if (App.IsDebugMode)
                {
                    DisplayAlert(LN.ErrorDetails, ex.ToString(), LN.Ok);
                }

                LogException(ex);
            });
        }

        protected override async void OnAppearing()
        {
            var migrationsToApply = new List<BaseMigration>();
            foreach (var migration in BaseMigration.Migrations)
            {
                if (migration.IsNeedsToBeApplied())
                {
                    migrationsToApply.Add(migration);
                }
            }

            if (migrationsToApply.Count > 0)
            {
                await App.Current.MainPage.DisplayAlert(LN.FinishingUpdateTitle, LN.FinishingUpdateDescription, LN.Ok);
                bool isSuccess = true;
                foreach (var migration in migrationsToApply)
                {
                    if (!migration.Apply())
                    {
                        isSuccess = false;
                    }
                }
                if (!isSuccess)
                {
                    await App.Current.MainPage.DisplayAlert(LN.FinishingUpdateTitle, LN.FinishingUpdateFail, LN.Ok);
                }
                var timetablePage = new TimetablePage();
                timetablePage.BindingContext = new TimetableViewModel(timetablePage.Navigation, timetablePage);
                Detail = new NavigationPage(timetablePage);
            }

            base.OnAppearing();
        }
        
        private static void LogException(Exception ex)
        {
#if !DEBUG
            // Getting exception Data
            var properties = new Dictionary<string, string>();
            foreach (DictionaryEntry de in ex.Data)
            {
                properties.Add(de.Key.ToString(), de.Value.ToString());
            }

            // Special cases for certain exception types
            if (ex is WebException webException)
            {
                // WebException happens for external reasons, and shouldn't be treated as an exception.
                // But just in case it is logged as Event

                properties.Add("Status", webException.Status.ToString());
                properties.Add("Message", webException.Message);

                if (webException.IsNoInternet())
                {
                    // Most likely device doesn't have internet connection
                    return;
                }

                Analytics.TrackEvent("WebException", properties);
                return;
            }

            // Logging exception
            Crashes.TrackError(ex, properties);
#endif
        }

        public async Task NavigateFromMenu(int id)
        {
            if (!MenuPages.ContainsKey(id))
            {
                switch (id)
                {
                    case (int)MenuItemType.Timetable:
                        var timetablePage = new TimetablePage();
                        timetablePage.BindingContext = new TimetableViewModel(timetablePage.Navigation, timetablePage);
                        MenuPages.Add(id, new NavigationPage(timetablePage));
                        break;
                    case (int)MenuItemType.About:
                        MenuPages.Add(id, new NavigationPage(new AboutPage
                        {
                            BindingContext = new AboutViewModel(Navigation)
                        }));
                        break;
                    case (int)MenuItemType.Donate:
                        MenuPages.Add(id, new NavigationPage(new DonatePage
                        {
                            BindingContext = new DonateViewModel(Navigation)
                        }));
                        break;
                    default:
                        throw new ArgumentException("Unknown menu page");
                }
            }

            var newPage = MenuPages[id];
            if (newPage != null && Detail != newPage)
            {
                Detail = newPage;

                if (Device.RuntimePlatform == Device.Android)
                {
                    await Task.Delay(100);
                }
                IsPresented = false;
            }
        }
    }
}