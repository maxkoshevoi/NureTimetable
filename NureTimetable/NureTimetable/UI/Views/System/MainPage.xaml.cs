using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using NureTimetable.Core.Localization;
using NureTimetable.Core.Models.Consts;
using NureTimetable.Core.Models.Exceptions;
using NureTimetable.Migrations;
using NureTimetable.UI.Themes;
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
using Xamarin.Essentials;
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
            base.OnAppearing();

            ThemeHelper.SetAppTheme(App.Current.RequestedTheme);
            App.Current.RequestedThemeChanged += (_, e) => ThemeHelper.SetAppTheme(e.RequestedTheme);
            
            if (!VersionTracking.IsFirstLaunchForCurrentBuild)
            {
                return;
            }

            var migrationsToApply = new List<BaseMigration>();
            foreach (var migration in BaseMigration.Migrations.Where(m => m.IsNeedsToBeApplied()))
            {
                migrationsToApply.Add(migration);
            }

            if (migrationsToApply.Any())
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
        }

        private static void LogException(Exception ex)
        {
            // Getting exception Data
            var properties = new Dictionary<string, string>();
            var attachments = new List<ErrorAttachmentLog>();
            foreach (DictionaryEntry de in ex.Data)
            {
                if (de.Value is ErrorAttachmentLog attachment)
                {
                    attachments.Add(attachment);
                    continue;
                }
                properties.Add(de.Key.ToString(), de.Value.ToString());
            }
            if (ex.InnerException != null)
            {
                attachments.Add(ErrorAttachmentLog.AttachmentWithText(ex.InnerException.ToString(), "InnerException.txt"));
            }

            // Special cases for certain exception types
            if (ex is WebException webEx)
            {
                if (Connectivity.NetworkAccess == NetworkAccess.None)
                {
                    // No internet caused WebException, nothing to log here
                    return;
                }

                // WebException happens for external reasons, and shouldn't be treated as an exception.
                // But just in case it is logged as Event

                if (webEx.Status != 0 && webEx.Status != WebExceptionStatus.UnknownError)
                {
                    properties.Add("Status", webEx.Status.ToString());
                }
                if (webEx.InnerException != null)
                {
                    properties.Add("InnerException", webEx.InnerException.GetType().FullName);
                }
                properties.Add("Message", ex.Message);

                Analytics.TrackEvent("WebException", properties);
                return;
            }
            else if (ex is CistOutOfMemoryException)
            {
                // CistOutOfMemoryException happens for external reasons, and shouldn't be treated as an exception.
                // But just in case it is logged as Event

                Analytics.TrackEvent("CistOutOfMemoryException", properties);
                return;
            }

            // Logging exception
            Crashes.TrackError(ex, properties, attachments.ToArray());
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

                if (DeviceInfo.Platform == DevicePlatform.Android)
                {
                    await Task.Delay(100);
                }
                IsPresented = false;
            }
        }
    }
}