﻿using NureTimetable.UI.ViewModels;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Forms;

namespace NureTimetable.UI.Views;

public partial class TimetablePage : ContentPage, ITimetablePageCommands
{
    public TimetablePage()
    {
        InitializeComponent();
        BindingContext = new TimetableViewModel(this);
    }

    public void TimetableNavigateTo(DateTime date) => Timetable.NavigateTo(date);

    public Task ScaleTodayButtonTo(double scale) => BToday.ScaleTo(scale);

    public async Task DisplayToastAsync(string message, int durationMilliseconds = 3000)
    {
        VisualElement anchor = this;
        if (BToday.Scale > 0)
        {
            anchor = BToday;
        }

        await anchor.DisplayToastAsync(message);
    }
}