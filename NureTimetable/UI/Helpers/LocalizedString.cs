using System.ComponentModel;

namespace NureTimetable.UI.Helpers;

[INotifyPropertyChanged]
public partial class LocalizedString
{
    private readonly Func<string> generator;

    public LocalizedString(Func<string> generator)
        : this(LocalizationResourceManager.Current, generator)
    {
    }

    public LocalizedString(LocalizationResourceManager localizationManager, Func<string> generator)
    {
        this.generator = generator;
        localizationManager.WeakSubscribe(this, static (subscriber, _, _) => subscriber.OnPropertyChanged((string?)null));
    }

    public string Localized => generator();

    public static implicit operator LocalizedString(Func<string> func) => new(func);
}


public static class INotifyPropertyChangedExtension
{
    public static void WeakSubscribe<T>(this INotifyPropertyChanged target, T subscriber, Action<T, object?, PropertyChangedEventArgs> action)
    {
        _ = target ?? throw new ArgumentNullException(nameof(target));
        if (subscriber == null || action == null)
        {
            return;
        }
        var weakSubscriber = new WeakReference(subscriber, false);
        target.PropertyChanged += handler;
        void handler(object? sender, PropertyChangedEventArgs e)
        {
            var s = (T?)weakSubscriber.Target;
            if (s == null)
            {
                target.PropertyChanged -= handler;
                return;
            }
            action(s, sender, e);
        }
    }
}