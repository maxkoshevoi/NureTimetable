using NureTimetable.Core.Models.InterplatformCommunication;
using NureTimetable.Models.Consts;
using Rg.Plugins.Popup.Pages;
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
        }

        protected override void OnAppearingAnimationEnd()
        {
            barManager.SetNavigationBarColor(Color.White.ToHex());
        }

        protected override void OnDisappearingAnimationBegin()
        {
            barManager.SetNavigationBarColor(ResourceManager.NavigationBarColor.ToHex());
        }
    }
}