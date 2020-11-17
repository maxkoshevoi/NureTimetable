using NureTimetable.UI.ViewModels.Info;
using Xamarin.Forms;

namespace NureTimetable.UI.Views
{
    public partial class DonatePage : ContentPage
	{
		public DonatePage()
		{
			InitializeComponent();
			BindingContext = new DonateViewModel();
		}
    }
}