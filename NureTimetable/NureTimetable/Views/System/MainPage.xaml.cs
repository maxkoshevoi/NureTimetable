using NureTimetable.Models.Consts;
using NureTimetable.Models.System;
using NureTimetable.Views.Info;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NureTimetable.UI.Views.Info;
using NureTimetable.ViewModels;
using NureTimetable.ViewModels.Info;
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
                    DisplayAlert("Детали ошибки:", ex.ToString(), "Ok");
                }
            });
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