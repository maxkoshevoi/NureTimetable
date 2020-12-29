using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NureTimetable.Core.Localization
{
    [ContentProperty(nameof(Text))]
    public class TranslateExtension : IMarkupExtension
    {
        private static readonly Lazy<ResourceManager> resmgr = new(
            () =>  new ResourceManager(typeof(LN).FullName, typeof(TranslateExtension).GetTypeInfo().Assembly));

        public string Text { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Text is null)
                return string.Empty;

            CultureInfo ci = LN.Culture;
            string translation = resmgr.Value.GetString(Text, ci) ?? Text;

            return translation;
        }
    }
}
