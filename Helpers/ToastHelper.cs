using System.Windows;
using System.Threading.Tasks;

namespace CryptoDashboard.Helpers
{
    public static class ToastHelper
    {
        public static async Task ShowToastAsync(string message, int durationMs = 1500)
        {
            var toast = new Window
            {
                Width = 250,
                Height = 60,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = System.Windows.Media.Brushes.Black,
                Opacity = 0.8,
                Topmost = true,
                ShowInTaskbar = false,
                Content = new System.Windows.Controls.TextBlock
                {
                    Text = message,
                    Foreground = System.Windows.Media.Brushes.White,
                    FontSize = 14,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextWrapping = System.Windows.TextWrapping.Wrap
                }
            };

            // Position in bottom right corner
            var desktop = System.Windows.SystemParameters.WorkArea;
            toast.Left = desktop.Right - toast.Width - 20;
            toast.Top = desktop.Bottom - toast.Height - 20;

            toast.Show();

            await Task.Delay(durationMs);

            toast.Close();
        }
    }
}
