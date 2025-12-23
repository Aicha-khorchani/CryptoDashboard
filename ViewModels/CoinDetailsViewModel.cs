using CryptoDashboard.Models;
using CryptoDashboard.Helpers;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CryptoDashboard.Services;
using System.Threading;
using System.Windows.Input;

namespace CryptoDashboard.ViewModels
{
    public class CoinDetailsViewModel : BaseViewModel
    {
        private readonly CoinGeckoService _coinService;
        private CancellationTokenSource? _historyCts;
        private int _currentDays = 30;
        public ICommand ChangeDaysCommand { get; }

        public CoinDetailsViewModel(Coin coin)
        {
            Coin = coin;
            _coinService = new CoinGeckoService();

            PlotModel = new PlotModel
            {
                Title = coin.Name + " - Price Chart",
                Background = OxyColors.Black,
                TextColor = OxyColors.White
            };

            // X-axis (date)
            PlotModel.Axes.Add(new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "dd/MM",
                Title = "Date",
                TextColor = OxyColors.White,
                TicklineColor = OxyColors.White,
                AxislineColor = OxyColors.White
            });

            // Left axis (price)
            PlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Price",
                TextColor = OxyColors.White,
                TicklineColor = OxyColors.White,
                AxislineColor = OxyColors.White
            });

            // Right axis (volume) - only used for 30d chart
            PlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Right,
                Title = "Volume",
                Key = "VolumeAxis",
                MajorGridlineStyle = LineStyle.None,
                MinorGridlineStyle = LineStyle.None
            });

            ChangeDaysCommand = new RelayCommand(async param =>
            {
                if (param == null) return;

                if (int.TryParse(param.ToString(), out int days))
                {
                    _currentDays = days;
                    await LoadHistoryAsync(days);
                }
            });
        }

        public Coin Coin { get; }
        public string Name => Coin.Name;
        public string Symbol => Coin.Symbol.ToUpper();

        public decimal CurrentPrice => Coin.CurrentPrice;
        public decimal PriceChange24h => Coin.PriceChangePercentage24h;
        public decimal MarketCap => Coin.MarketCap;
        public decimal TotalVolume => Coin.TotalVolume;

        public string ImageUrl => Coin.ImageUrl;
        private bool _show30DayExplanation;
        public bool Show30DayExplanation
        {
            get => _show30DayExplanation;
            set => SetProperty(ref _show30DayExplanation, value);
        }

        private string _chartExplanationText = "";
        public string ChartExplanationText
        {
            get => _chartExplanationText;
            set => SetProperty(ref _chartExplanationText, value);
        }


        public PlotModel PlotModel { get; }

        private bool _toastShown = false;
        private bool _isLoadingHistory = false;

        public async Task LoadHistoryAsync(int days = 30)
        {
            try
            {
                // Wait if a load is in progress
                while (_isLoadingHistory)
                {
                    if (!_toastShown)
                    {
                        _toastShown = true;
                        _ = ToastHelper.ShowToastAsync("Please wait, loading chart...");
                    }
                    await Task.Delay(100);
                }
                _toastShown = false;

                _isLoadingHistory = true;
                _historyCts?.Cancel();
                _historyCts = new CancellationTokenSource();

                var history = await _coinService.GetMarketChartAsync(Coin.Id, days, "usd", _historyCts.Token);

                await App.Current.Dispatcher.InvokeAsync(() =>
                {
                    // Clear previous series
                    PlotModel.Series.Clear();

                    // --- PRICE LINE ---
                    var priceSeries = new LineSeries
                    {
                        Title = Coin.Name,
                        Color = OxyColors.SteelBlue,
                        StrokeThickness = 2
                    };
                    foreach (var point in history)
                        priceSeries.Points.Add(DateTimeAxis.CreateDataPoint(point.Date, (double)point.Price));
                    PlotModel.Series.Add(priceSeries);

                    // --- ONLY 30-DAY CHART: MA7 + VOLUME ---
                    if (days == 30)
                    {
                        // MA7
                        var maSeries = new LineSeries
                        {
                            Title = "MA (7)",
                            Color = OxyColors.Orange,
                            StrokeThickness = 2,
                            LineStyle = LineStyle.Dash
                        };
                        int period = 7;
                        for (int i = period - 1; i < history.Count; i++)
                        {
                            decimal sum = 0;
                            for (int j = i - period + 1; j <= i; j++)
                                sum += history[j].Price;
                            maSeries.Points.Add(DateTimeAxis.CreateDataPoint(history[i].Date, (double)(sum / period)));
                        }
                        PlotModel.Series.Add(maSeries);

                        // Volume bars
                        var volumeSeries = new RectangleBarSeries
                        {
                            Title = "Volume",
                            YAxisKey = "VolumeAxis",
                            FillColor = OxyColor.FromAColor(120, OxyColors.Gray)
                        };
                        for (int i = 0; i < history.Count; i++)
                        {
                            double x = DateTimeAxis.ToDouble(history[i].Date);
                            volumeSeries.Items.Add(new RectangleBarItem
                            {
                                X0 = x - 0.3,
                                X1 = x + 0.3,
                                Y0 = 0,
                                Y1 = (double)history[i].Volume
                            });
                        }
                        PlotModel.Series.Add(volumeSeries);
                    }

                    PlotModel.InvalidatePlot(true);
if (days == 30)
{
    Show30DayExplanation = true;
    ChartExplanationText =
        "Blue line: Daily closing price\n" +
        "Orange dashed line: 7-day moving average (MA7)\n" +
        "Gray bars: Trading volume\n" +
        "Left axis: Price (USD), Right axis: Volume (USD)";
}
else
{
    Show30DayExplanation = false;
    ChartExplanationText = "";
}

                    // Notify properties
                    OnPropertyChanged(nameof(CurrentPrice));
                    OnPropertyChanged(nameof(PriceChange24h));
                    OnPropertyChanged(nameof(MarketCap));
                    OnPropertyChanged(nameof(TotalVolume));
                });
            }
            catch (OperationCanceledException) { /* expected */ }
            catch (Exception ex)
            {
                Logger.Log("CoinDetails history failed: " + ex);
            }
            finally
            {
                _isLoadingHistory = false;
            }
        }
    }
}
