namespace NureTimetable.UI.Helpers;

[ContentProperty(nameof(Text))]
public class TranslateExtension : IMarkupExtension<BindingBase>
{
    public string Text { get; set; } = string.Empty;

    public string? StringFormat { get; set; }

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);

    public BindingBase ProvideValue(IServiceProvider serviceProvider)
    {
        var binding = new Binding
        {
            Mode = BindingMode.OneWay,
            Path = $"[{Text}]",
            Source = LocalizationResourceManager.Current,
            StringFormat = StringFormat
        };
        return binding;
    }
}
