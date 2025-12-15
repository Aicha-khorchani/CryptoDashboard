using System;
using System.IO;

namespace CryptoDashboard.Helpers
{
    public static class Logger
    {
        private static readonly string PathLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");
        private static readonly object _lock = new object();

        public static void Log(string message)
        {
            try
            {
                lock (_lock)
                {
                    File.AppendAllText(PathLog, $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
                }
            }
            catch { }
        }
    }
}
