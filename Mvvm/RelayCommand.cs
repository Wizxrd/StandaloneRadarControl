using System.Windows.Input;
namespace Mvvm;

public class RelayCommand : ICommand
{
    private readonly Action<object?> execute;
    private readonly Func<object?, bool> canExecute;

    public event EventHandler? CanExecuteChanged;

    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        this.execute = execute;
        this.canExecute = canExecute ?? (_ => true);
    }

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        this.execute = _ => execute();
        this.canExecute = canExecute != null ? _ => canExecute() : _ => true;
    }

    public bool CanExecute(object? parameter) => canExecute(parameter);
    public void Execute(object? parameter) => execute(parameter);
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

public class RelayCommand<T> : ICommand
{
    private readonly Action<T> execute;
    private readonly Predicate<T>? canExecute;

    public RelayCommand(Action<T> execute, Predicate<T>? canExecute = null)
    {
        this.execute = execute;
        this.canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) =>
        parameter is T t && (canExecute?.Invoke(t) ?? true);

    public void Execute(object? parameter)
    {
        if (parameter is T t)
            execute(t);
    }

    public event EventHandler? CanExecuteChanged;
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
