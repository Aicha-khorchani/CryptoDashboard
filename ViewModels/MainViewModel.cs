using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Threading;
using CryptoDashboard.Helpers;
using CryptoDashboard.Models;
using CryptoDashboard.Services;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace CryptoDashboard.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly CoinGeckoService _coinService;
        private readonly FavoritesService _favoritesService;
        private readonly Dispatcher _dispatcher;

        private bool _isLoading;
        private string _searchQuery = string.Empty;
        private Coin? _selectedCoin;

        private ObservableCollection<Coin> _allCoins = new();

        public ObservableCollection<Coin> Coins { get; } = new();
        public PlotModel PlotModel { get; }
        public AsyncRelayCommand RefreshCommand { get; }

        public MainViewModel()
        {
            _coinService = new CoinGeckoService();
            _favoritesService = new FavoritesService();
            _dispatcher = Dispatcher.CurrentDispatcher;

            RefreshCommand = new AsyncRelayCommand(LoadCoinsAsync);

            PlotModel = new PlotModel
            {
                Title = "Price Chart",
                Background = OxyColors.Black,
                TextColor = OxyColors.White
            };

            PlotModel.Axes.Add(new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "dd/MM",
                Title = "Date",
                TextColor = OxyColors.White,
                TicklineColor = OxyColors.White,
                AxislineColor = OxyColors.White
            });

            PlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Price",
                TextColor = OxyColors.White,
                TicklineColor = OxyColors.White,
                AxislineColor = OxyColors.White
            });
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (SetProperty(ref _searchQuery, value))
                {
                    Debug.WriteLine($"SearchQuery changed: {_searchQuery}");
                    FilterCoins();
                }
            }
        }

        public Coin? SelectedCoin
        {
            get => _selectedCoin;
            set
            {
                if (SetProperty(ref _selectedCoin, value) && value != null)
                {
                    Debug.WriteLine($"SelectedCoin changed: {value.Name} ({value.Symbol})");
                    // Safe fire-and-forget async call
                    _ = LoadCoinHistoryAsync(value);
                }
            }
        }

        // ✅ Make public so MainWindow can call it
        public async Task LoadCoinsAsync()
        {
            try
            {
                Debug.WriteLine("Loading coins...");
                IsLoading = true;

                var coins = await _coinService.GetMarketCoinsAsync();

                if (coins == null)
                    coins = new System.Collections.Generic.List<Coin>();

                await _dispatcher.InvokeAsync(() =>
                {
                    Coins.Clear();
                    _allCoins.Clear();
                    foreach (var coin in coins)
                    {
                        _allCoins.Add(coin);
                        Coins.Add(coin);
                        Debug.WriteLine($"Coin added: {coin.Name} ({coin.Symbol}) - {coin.CurrentPrice}");
                    }
                });

                Debug.WriteLine("Finished loading coins.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error loading coins: " + ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void FilterCoins()
        {
            _dispatcher.Invoke(() =>
            {
                Coins.Clear();
                foreach (var coin in _allCoins)
                {
                    if (string.IsNullOrWhiteSpace(SearchQuery) ||
                        coin.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                        coin.Symbol.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
                    {
                        Coins.Add(coin);
                        Debug.WriteLine($"Coin matched filter: {coin.Name} ({coin.Symbol})");
                    }
                }
            });
        }

        private async Task LoadCoinHistoryAsync(Coin coin)
        {
            try
            {
                Debug.WriteLine($"Loading history for {coin.Name}...");
                var history = await _coinService.GetMarketChartAsync(coin.Id, 7);

                if (history == null)
                    history = new System.Collections.Generic.List<PricePoint>();

                await _dispatcher.InvokeAsync(() =>
                {
                    PlotModel.Series.Clear();
                    var series = new LineSeries
                    {
                        Title = coin.Name,
                        StrokeThickness = 2,
                        Color = OxyColors.SteelBlue
                    };

                    foreach (var point in history)
                    {
                        series.Points.Add(DateTimeAxis.CreateDataPoint(point.Date, (double)point.Price));
                        Debug.WriteLine($"Point added: {point.Date:dd/MM} -> {point.Price}");
                    }

                    PlotModel.Series.Add(series);
                    PlotModel.InvalidatePlot(true);
                });

                Debug.WriteLine($"Finished loading history for {coin.Name}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading history for {coin.Name}: {ex}");
            }
        }
    }
}
