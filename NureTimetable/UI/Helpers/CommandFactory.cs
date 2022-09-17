namespace NureTimetable.UI.Helpers;

public static class CommandFactory
{
    public static IRelayCommand Create(Action execute, Func<bool>? canExecute = null) =>
        new RelayCommand(execute, canExecute ?? (() => true));

    public static IRelayCommand<TExecute> Create<TExecute>(Action<TExecute?> execute, Func<bool>? canExecute = null) =>
        new RelayCommand<TExecute>(execute, ConvertCanExecute<TExecute?>(canExecute));

    public static IRelayCommand<TExecute> Create<TExecute, TCanExecute>(Action<TExecute?> execute, Predicate<TExecute?> canExecute) =>
        new RelayCommand<TExecute>(execute, canExecute);

    public static IAsyncRelayCommand Create(
        Func<Task> execute,
        Func<bool>? canExecute = null) =>
        new AsyncRelayCommand(execute, canExecute ?? (() => true));

    public static IAsyncRelayCommand<TExecute> Create<TExecute>(
        Func<TExecute?, Task> execute,
        Func<bool>? canExecute = null) =>
        new AsyncRelayCommand<TExecute>(execute, ConvertCanExecute<TExecute?>(canExecute));

    private static Predicate<T> ConvertCanExecute<T>(Func<bool>? canExecute) => 
        canExecute == null ? _ => true : _ => canExecute();
}
