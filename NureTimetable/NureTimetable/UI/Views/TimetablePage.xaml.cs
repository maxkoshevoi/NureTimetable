using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NureTimetable.UI.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TimetablePage : ContentPage, ITimetablePageCommands
    {
        public TimetablePage()
        {
            InitializeComponent();
        }

        public void TimetableNavigateTo(DateTime date)
        {
            Timetable.NavigateTo(date);
        }

        public async Task ScaleTodayButtonTo(double scale)
        {
            await BToday.ScaleTo(scale);
        }
    }
}