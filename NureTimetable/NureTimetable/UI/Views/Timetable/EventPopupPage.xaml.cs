using NureTimetable.Core.Models.Consts;
using NureTimetable.Core.Models.InterplatformCommunication;
using NureTimetable.Core.Models.Settings;
using NureTimetable.Models.Consts;
using Rg.Plugins.Popup.Pages;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace NureTimetable.UI.Views
{
    public partial class EventPopupPage : PopupPage
    {
        readonly IBarStyleManager barManager;

        public EventPopupPage()
        {
            InitializeComponent();
            barManager = DependencyService.Get<IBarStyleManager>();

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
            barManager.SetNavigationBarColor(ResourceManager.NavigationBarColor.ToHex());
        }

        private void SetCustomNavigationBar() => barManager.SetNavigationBarColor(Color.White.ToHex());
    }
}