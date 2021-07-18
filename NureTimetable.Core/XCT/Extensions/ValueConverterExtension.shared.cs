using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using System;

namespace Xamarin.CommunityToolkit.Extensions.Internals
{
    public abstract class ValueConverterExtension : BindableObject, IMarkupExtension<IValueConverter>
	{
		public IValueConverter ProvideValue(IServiceProvider serviceProvider)
			=> (IValueConverter)this;

		object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
			=> ((IMarkupExtension<IValueConverter>)this).ProvideValue(serviceProvider);
	}
}