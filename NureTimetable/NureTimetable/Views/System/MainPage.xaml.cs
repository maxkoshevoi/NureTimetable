using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using NureTimetable.Core.Localization;
using NureTimetable.Core.Models.Consts;
using NureTimetable.Models.System;
using NureTimetable.UI.Views.Info;
using NureTimetable.ViewModels.Info;
using NureTimetable.Views.Info;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NureTimetable.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : MasterDetailPage
    {
        Dictionary<int, NavigationPage> MenuPages = new Dictionary<int, NavigationPage>();
        public MainPage()
        {
            InitializeComponent();

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
                // WebException happens for external reasons, and should't be treated as an exception.
                // But just in case it is logged as Event

                properties.Add("Status", webException.Status.ToString());
                properties.Add("Message", webException.Message);

                if (new[] { WebExceptionStatus.NameResolutionFailure, WebExceptionStatus.ConnectFailure }.Contains(webException.Status))
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
                        MenuPages.Add(id, new NavigationPage(new TimetablePage()));
                        break;
                    case (int)MenuItemType.About:
                        MenuPages.Add(id, new NavigationPage(new AboutPage()
                        {
                            BindingContext = new AboutViewModel()
                        }));
                        break;
                    case (int)MenuItemType.Donate:
                        MenuPages.Add(id, new NavigationPage(new DonatePage()));
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
                    await Task.Delay(100);

                IsPresented = false;
            }
        }
    }
}