using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CryptoDashboard.Helpers
{
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private bool _isExecuting;

        public AsyncRelayCommand(Func<Task> execute)
        {
            _execute = execute;
        }

        public bool CanExecute(object? parameter)
            => !_isExecuting;

        public async void Execute(object? parameter)
        {
            if (_isExecuting)
                return;

            try
            {
                _isExecuting = true;
                CommandManager.InvalidateRequerySuggested();
                await _execute();
            }
            catch (Exception ex)
            {
                // TEMP: log to console (later we show dialog)
                Console.WriteLine(ex);
            }
            finally
            {
                _isExecuting = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
