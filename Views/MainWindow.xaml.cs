using System;
using System.Windows;
using CryptoDashboard.ViewModels;

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
                Console.WriteLine("Window loaded, starting to load coins...");
                await vm.LoadCoinsAsync();
            };
        }
    }
}
