using NureTimetable.Core.Models;
using System;
using Xamarin.CommunityToolkit.Helpers;

namespace NureTimetable.UI.Helpers
{
    public class LocalizedString : NotifyPropertyChangedBase
    {
        public LocalizedString(Func<string> generator = null)
        {
            Generator = generator;
            LocalizationResourceManager.Current.PropertyChanged += (_, _) => OnPropertyChanged(nameof(Localized));
        }

        public Func<string> generator;
        public Func<string> Generator { get => generator; set => SetProperty(ref generator, value, propertyName: nameof(Localized)); }

        public string Localized => Generator?.Invoke();

        public static implicit operator LocalizedString(Func<string> func) => new(func);
    }
}
