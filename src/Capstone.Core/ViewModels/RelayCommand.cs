using System.Windows.Input;

namespace Capstone.Core.ViewModels;

/// <summary>
/// A button's behaviour as two delegates: what it does, and whether it can right now.
/// The view model calls <see cref="RaiseCanExecuteChanged"/> when the answer to "can it?" may have moved.
/// </summary>
public sealed class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool> _canExecute;

    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute ?? (_ => true);
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => _canExecute(parameter);

    /// <summary>Does nothing when <see cref="CanExecute"/> says no: bindings don't always ask first.</summary>
    public void Execute(object? parameter)
    {
        if (CanExecute(parameter))
            _execute(parameter);
    }

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
