using NureTimetable.UI.ViewModels;
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