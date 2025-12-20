using System;
using System.Windows;
using CryptoDashboard.ViewModels;
using System.Windows.Input;

namespace CryptoDashboard.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Set DataContext
            var vm = new MainViewModel();
            this.DataContext = vm;

            // Call LoadCoinsAsync after window loads
            this.Loaded += async (sender, e) =>
            {
                await vm.LoadCoinsAsync();
            };
        }
        

private void Close_Click(object sender, RoutedEventArgs e)
{
    Close();
}

private void Minimize_Click(object sender, RoutedEventArgs e)
{
    WindowState = WindowState.Minimized;
}

private void Maximize_Click(object sender, RoutedEventArgs e)
{
    WindowState = WindowState == WindowState.Maximized
        ? WindowState.Normal
        : WindowState.Maximized;
}

private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
{
    if (e.ClickCount == 2)
    {
        Maximize_Click(sender, e);
    }
    else
    {
        DragMove();
    }
}

    }
    

}
