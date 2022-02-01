using NureTimetable.Core.Models.Consts;
using NureTimetable.DAL.Settings.Models;
using NureTimetable.Models.Consts;
using Rg.Plugins.Popup.Pages;
using Xamarin.CommunityToolkit.PlatformConfiguration.AndroidSpecific;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;

namespace NureTimetable.UI.Views
{
    public partial class EventPopupPage : PopupPage
    {
        public EventPopupPage()
        {
            InitializeComponent();

            MessagingCenter.Subscribe<Application, AppTheme>(this, MessageTypes.ThemeChanged, (_, _) =>
                SetCustomNavigationBar());
        }

        protected override async void OnAppearingAnimationBegin()
        {
            await Task.Delay(100);
            SetCustomNavigationBar();
        }

        protected override async void OnDisappearingAnimationBegin()
        {
            MessagingCenter.Unsubscribe<Application, AppTheme>(this, MessageTypes.ThemeChanged);

            await Task.Delay(50);

            On<Android>().SetNavigationBarColor(ResourceManager.NavigationBarColor);
            On<Android>().SetNavigationBarStyle(ResourceManager.NavigationBarStyle);
        }

        private void SetCustomNavigationBar()
        {
            On<Android>().SetNavigationBarColor(Color.White);
            On<Android>().SetNavigationBarStyle(NavigationBarStyle.DarkContent);
        }
    }
}