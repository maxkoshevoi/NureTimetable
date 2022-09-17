using System.Resources;

namespace NureTimetable.UI.Helpers;

[INotifyPropertyChanged]
public partial class LocalizationResourceManager
{
    private static readonly Lazy<LocalizationResourceManager> currentHolder = new(() => new LocalizationResourceManager());

    public static LocalizationResourceManager Current => currentHolder.Value;

    private ResourceManager? resourceManager;
    private CultureInfo currentCulture = Thread.CurrentThread.CurrentUICulture;

    private LocalizationResourceManager()
    {
    }

    public void Init(ResourceManager resource) => resourceManager = resource;

    public void Init(ResourceManager resource, CultureInfo initialCulture)
    {
        CurrentCulture = initialCulture;
        Init(resource);
    }

    public string GetValue(string text)
    {
        if (resourceManager == null)
            throw new InvalidOperationException($"Must call {nameof(LocalizationResourceManager)}.{nameof(Init)} first");

        return resourceManager.GetString(text, CurrentCulture) ?? throw new NullReferenceException($"{nameof(text)}: {text} not found");
    }

    public string this[string text] => GetValue(text);

    public CultureInfo CurrentCulture
    {
        get => currentCulture;
        set => SetProperty(ref currentCulture, value, null);
    }
}
