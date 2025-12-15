using System;
using System.Collections.Generic;
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
        private string? _lastCoinId;
        private bool _isLoadingChart;

        private bool _isLoading;
        private string _searchQuery = string.Empty;
        private CoinViewModel? _selectedCoin;
        private List<string> _favoriteIds;

        private ObservableCollection<CoinViewModel> _allCoins = new();

        public ObservableCollection<CoinViewModel> Coins { get; } = new();
        public PlotModel PlotModel { get; }
        public AsyncRelayCommand RefreshCommand { get; }
        public RelayCommand ToggleFavoriteCommand { get; }

        public MainViewModel()
        {
            _coinService = new CoinGeckoService();
            _favoritesService = new FavoritesService();
            _dispatcher = Dispatcher.CurrentDispatcher;

            RefreshCommand = new AsyncRelayCommand(LoadCoinsAsync);
            ToggleFavoriteCommand = new RelayCommand(param =>
            {
               if (param is CoinViewModel coinVM)
               ToggleFavorite(coinVM);
            });

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

            _favoriteIds = _favoritesService.LoadFavorites();
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
                    FilterCoins();
                }
            }
        }

        public CoinViewModel? SelectedCoin
        {
            get => _selectedCoin;
            set
            {
                if (SetProperty(ref _selectedCoin, value) && value != null)
                {
                    _ = LoadCoinHistoryAsync(value.GetModel());
                }
            }
        }

        public async Task LoadCoinsAsync()
        {
            IsLoading = true;

            var coins = await _coinService.GetMarketCoinsAsync() ?? new List<Coin>();

            await _dispatcher.InvokeAsync(() =>
            {
                Coins.Clear();
                _allCoins.Clear();

                foreach (var coin in coins)
                {
                    coin.IsFavorite = _favoriteIds.Contains(coin.Id);
                    var vm = new CoinViewModel(coin);
                    _allCoins.Add(vm);
                    Coins.Add(vm);
                }
            });

            IsLoading = false;
        }

        public void ToggleFavorite(CoinViewModel coinVM)
        {
            if (coinVM == null) return;

            bool wasFav = _favoriteIds.Contains(coinVM.Id);

            if (wasFav)
                _favoriteIds.Remove(coinVM.Id);
            else
                _favoriteIds.Add(coinVM.Id);

            coinVM.IsFavorite = !wasFav;

            _favoritesService.SaveFavorites(_favoriteIds);
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
                        coin.Id.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
                    {
                        Coins.Add(coin);
                    }
                }
            });
        }

        private async Task LoadCoinHistoryAsync(Coin coin)
        {
            if (_lastCoinId == coin.Id || _isLoadingChart) return;

            _lastCoinId = coin.Id;
            _isLoadingChart = true;

            try
            {
                var history = await _coinService.GetMarketChartAsync(coin.Id, 7) ?? new List<PricePoint>();

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
                        series.Points.Add(DateTimeAxis.CreateDataPoint(point.Date, (double)point.Price));

                    PlotModel.Series.Add(series);
                    PlotModel.InvalidatePlot(true);
                });
            }
            finally
            {
                _isLoadingChart = false;
            }
        }
    }
}
