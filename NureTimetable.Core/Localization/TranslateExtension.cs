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
        const string ResourceId = "NureTimetable.Core.Localization.LN";

        static readonly Lazy<ResourceManager> resmgr =
            new Lazy<ResourceManager>(() =>
                new ResourceManager(ResourceId, typeof(TranslateExtension)
                        .GetTypeInfo().Assembly));

        public string Text { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Text is null)
                return "";

            CultureInfo ci = LN.Culture; //CrossMultilingual.Current.CurrentCultureInfo;
            string translation = resmgr.Value.GetString(Text, ci);

            if (translation is null)
            {
                translation = Text; // returns the key, which GETS DISPLAYED TO THE USER
            }
            return translation;
        }
    }
}
