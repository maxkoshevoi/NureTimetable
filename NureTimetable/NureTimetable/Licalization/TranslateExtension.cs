using System;
using System.Reflection;
using System.Resources;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NureTimetable.Licalization
{
    [ContentProperty(nameof(Text))]
    public class TranslateExtension : IMarkupExtension
    {
        const string ResourceId = "NureTimetable.Licalization.LN";

        static readonly Lazy<ResourceManager> resmgr =
            new Lazy<ResourceManager>(() =>
                new ResourceManager(ResourceId, typeof(TranslateExtension)
                        .GetTypeInfo().Assembly));

        public string Text { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Text == null)
                return "";

            var ci = LN.Culture; //CrossMultilingual.Current.CurrentCultureInfo;
            var translation = resmgr.Value.GetString(Text, ci);

            if (translation == null)
            {
                translation = Text; // returns the key, which GETS DISPLAYED TO THE USER
            }
            return translation;
        }
    }
}
