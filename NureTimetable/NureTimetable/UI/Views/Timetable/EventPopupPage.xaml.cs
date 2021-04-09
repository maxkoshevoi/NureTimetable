using NureTimetable.Core.Models.InterplatformCommunication;
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
        }

        protected override async void OnAppearingAnimationBegin()
        {
            await Task.Delay(100);
            barManager.SetNavigationBarColor(Color.White.ToHex());
        }

        protected override async void OnDisappearingAnimationBegin()
        {
            await Task.Delay(50);
            barManager.SetNavigationBarColor(ResourceManager.NavigationBarColor.ToHex());
        }
    }
}