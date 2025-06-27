using System;
using System.Windows.Input;

namespace FireApplications.Helpers
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?,bool>? _canExecute;
        public RelayCommand(Action<object?> exec, Func<object?,bool>? can = null)
        {
            _execute    = exec  ?? throw new ArgumentNullException(nameof(exec));
            _canExecute = can;
        }
        public bool CanExecute(object? p) => _canExecute?.Invoke(p) ?? true;
        public void Execute(object? p)    => _execute(p);
        public event EventHandler? CanExecuteChanged;
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}