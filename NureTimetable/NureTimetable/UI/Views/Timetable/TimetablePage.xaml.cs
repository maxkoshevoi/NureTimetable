using NureTimetable.UI.ViewModels.Timetable;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace NureTimetable.UI.Views
{
    public partial class TimetablePage : ContentPage, ITimetablePageCommands
    {
        public TimetablePage()
        {
            InitializeComponent();
            BindingContext = new TimetableViewModel(this);
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