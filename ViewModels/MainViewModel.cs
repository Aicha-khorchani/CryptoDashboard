using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Threading;
using CryptoDashboard.Helpers;
using CryptoDashboard.Models;
using CryptoDashboard.Services;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Threading;


namespace CryptoDashboard.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly CoinGeckoService _coinService;
        private readonly FavoritesService _favoritesService;
        private readonly Dispatcher _dispatcher;
        private string? _lastCoinId;
        private bool _isLoadingChart;
        private CancellationTokenSource? _chartCts;
        private CoinViewModel? _selectedCoin;
        private ObservableCollection<CoinViewModel> _favoriteCoins = new();
        public ObservableCollection<CoinViewModel> FavoriteCoins
        {
         get => _favoriteCoins;
         set => SetProperty(ref _favoriteCoins, value);
        }

        private bool _isLoading;
        private string _searchQuery = string.Empty;
        private List<string> _favoriteIds;

        private ObservableCollection<CoinViewModel> _allCoins = new();

        public ObservableCollection<CoinViewModel> Coins { get; } = new();

        public PlotModel MainPlotModel { get; }  // default chart
        public PlotModel CurrentPlotModel { get; set; } // bind PlotView to this

        public AsyncRelayCommand RefreshCommand { get; }
        public RelayCommand ToggleFavoriteCommand { get; }
        public RelayCommand ShowCoinDetailsCommand { get; }
        public RelayCommand ToggleChartCommand { get; }

        private CoinDetailsViewModel? _currentCoinDetails;
        public CoinDetailsViewModel? CurrentCoinDetails
        {
            get => _currentCoinDetails;
            set => SetProperty(ref _currentCoinDetails, value);
        }

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

            ShowCoinDetailsCommand = new RelayCommand(async param =>
            {
                if (param is CoinViewModel coinVM)
                    await ShowCoinDetailsAsync(coinVM.GetModel());
            });

            // Default chart
            MainPlotModel = new PlotModel
            {
                Title = "Price Chart",
                Background = OxyColors.Black,
                TextColor = OxyColors.White
            };
            MainPlotModel.Axes.Add(new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "dd/MM",
                Title = "Date",
                TextColor = OxyColors.White,
                TicklineColor = OxyColors.White,
                AxislineColor = OxyColors.White
            });
            MainPlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Price",
                TextColor = OxyColors.White,
                TicklineColor = OxyColors.White,
                AxislineColor = OxyColors.White
            });

ToggleChartCommand = new RelayCommand(_ =>
{
    if (CurrentPlotModel == MainPlotModel && CurrentCoinDetails != null)
        CurrentPlotModel = CurrentCoinDetails.PlotModel;
    else
        CurrentPlotModel = MainPlotModel;

    OnPropertyChanged(nameof(CurrentPlotModel));
});


            // Initially bind to main chart
            CurrentPlotModel = MainPlotModel;

            _favoriteIds = _favoritesService.LoadFavorites();
        }

        private async Task ShowCoinDetailsAsync(Coin coin)
        {
            if (coin == null) return;

            IsLoading = true;
            try
            {
                CurrentCoinDetails = new CoinDetailsViewModel(coin);
                await CurrentCoinDetails.LoadHistoryAsync(30);

                // Switch CurrentPlotModel to coin detail
                CurrentPlotModel = CurrentCoinDetails.PlotModel;
                OnPropertyChanged(nameof(CurrentPlotModel));
            }
            finally
            {
                IsLoading = false;
            }
        }
private void UpdateFavoriteCoins()
{
    FavoriteCoins.Clear();
    foreach (var coin in Coins)
    {
        if (coin.IsFavorite)
            FavoriteCoins.Add(coin);
    }
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
                    FilterCoins();
            }
        }

public CoinViewModel? SelectedCoin
{
    get => _selectedCoin;
    set
    {
        if (!SetProperty(ref _selectedCoin, value) || value == null)
            return;

_ = LoadChartSafe(value.GetModel());
    }
}

private async Task LoadChartSafe(Coin coin){
    try
    {
        _chartCts?.Cancel();
        _chartCts = new CancellationTokenSource();

        await Task.Delay(200, _chartCts.Token); // debounce

        var history = await _coinService
            .GetMarketChartAsync(coin.Id, 7, "usd", _chartCts.Token);

        if (_dispatcher.HasShutdownStarted) return;

        await _dispatcher.InvokeAsync(() =>
        {
            MainPlotModel.Series.Clear();

            var series = new LineSeries
            {
                Title = coin.Name,
                StrokeThickness = 2,
                Color = OxyColors.SteelBlue
            };

            foreach (var point in history)
                series.Points.Add(DateTimeAxis.CreateDataPoint(
                    point.Date, (double)point.Price));

            MainPlotModel.Series.Add(series);
            MainPlotModel.Title = $"{coin.Name} - 7 days";
            MainPlotModel.InvalidatePlot(true);

            CurrentPlotModel = MainPlotModel;
            OnPropertyChanged(nameof(CurrentPlotModel));
        });
    }
    catch (OperationCanceledException)
    {
        // expected
    }
    catch (Exception ex)
    {
        Logger.Log("Chart load failed: " + ex);
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

UpdateFavoriteCoins();

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

    UpdateFavoriteCoins();
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
    }
}
