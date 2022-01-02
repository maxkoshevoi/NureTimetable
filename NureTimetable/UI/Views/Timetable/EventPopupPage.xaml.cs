using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using NureTimetable.Core.Models.Consts;
using NureTimetable.DAL.Settings.Models;
using NureTimetable.UI.Models.Consts;
using Rg.Plugins.Popup.Pages;
using Xamarin.CommunityToolkit.PlatformConfiguration.AndroidSpecific;
using Platform = Microsoft.Maui.Controls.PlatformConfiguration;

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

        protected override async Task OnAppearingAnimationBeginAsync()
        {
            await Task.Delay(100);
            SetCustomNavigationBar();
        }

        protected override async Task OnDisappearingAnimationBeginAsync()
        {
            MessagingCenter.Unsubscribe<Application, AppTheme>(this, MessageTypes.ThemeChanged);

            await Task.Delay(50);

            On<Platform::Android>().SetNavigationBarColor(ResourceManager.NavigationBarColor);
            On<Platform::Android>().SetNavigationBarStyle(ResourceManager.NavigationBarStyle);
        }

        private void SetCustomNavigationBar()
        {
            On<Platform::Android>().SetNavigationBarColor(Colors.White);
            On<Platform::Android>().SetNavigationBarStyle(NavigationBarStyle.DarkContent);
        }
    }
}