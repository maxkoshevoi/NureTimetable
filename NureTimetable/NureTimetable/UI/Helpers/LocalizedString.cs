using System;
using System.ComponentModel;
using Xamarin.CommunityToolkit.Helpers;
using Xamarin.CommunityToolkit.ObjectModel;

namespace NureTimetable.UI.Helpers
{
	public class LocalizedString : ObservableObject
	{
		readonly Func<string> generator;

		public LocalizedString(Func<string> generator = null)
			: this(LocalizationResourceManager.Current, generator)
		{
		}

		public LocalizedString(LocalizationResourceManager localizationManager, Func<string> generator = null)
		{
			this.generator = generator;
			localizationManager.WeakSubscribe(this, (t, sender, e) => t.OnPropertyChanged(null));
		}

		public string Localized => generator?.Invoke();

		public static implicit operator LocalizedString(Func<string> func) => new LocalizedString(func);
	}

	public static class INotifyPropertyChangedEx
	{
		public static void WeakSubscribe<T>(this INotifyPropertyChanged target, T subscriber, Action<T, object, EventArgs> action)
		{
			_ = target ?? throw new ArgumentNullException(nameof(target));
			if (subscriber == null || action == null)
			{
				return;
			}

			var weakSubscriber = new WeakReference(subscriber, false);
			target.PropertyChanged += handler;

				void handler(object sender, PropertyChangedEventArgs e)
			{
				var s = (T)weakSubscriber.Target;
				if (s == null)
				{
					target.PropertyChanged -= handler;
					return;
				}

				action(s, sender, e);
			}
		}
	}
}
