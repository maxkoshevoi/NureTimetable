using System;
using System.ComponentModel;
using Xamarin.CommunityToolkit.Helpers;
using Xamarin.CommunityToolkit.ObjectModel;

namespace NureTimetable.UI.Helpers
{
	public class LocalizedString : ObservableObject
	{
        private readonly Func<string> generator;
        private readonly LocalizationResourceManager localizationManager;

		public LocalizedString(Func<string> generator = null)
			: this(LocalizationResourceManager.Current, generator)
		{
		}

		public LocalizedString(LocalizationResourceManager localizationManager, Func<string> generator = null)
		{
			this.localizationManager = localizationManager;
			this.generator = generator;
			localizationManager.PropertyChanged += Invalidate;
		}

		public string Localized => generator?.Invoke();

		public static implicit operator LocalizedString(Func<string> func) => new(func);

		void Invalidate(object sender, PropertyChangedEventArgs e) =>
			OnPropertyChanged(null);

		public void Dispose() => localizationManager.PropertyChanged -= Invalidate;

		~LocalizedString() => Dispose();
	}
}
