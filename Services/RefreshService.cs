using System;
using System.Windows.Threading;

namespace CryptoDashboard.Services
{
    public class RefreshService
    {
        private readonly DispatcherTimer _timer;

        public event Action? Tick;

        public RefreshService(TimeSpan interval)
        {
            _timer = new DispatcherTimer
            {
                Interval = interval
            };

            _timer.Tick += (_, _) => Tick?.Invoke();
        }

        public void Start() => _timer.Start();

        public void Stop() => _timer.Stop();
    }
}
