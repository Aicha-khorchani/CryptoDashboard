using CryptoDashboard.Models;
using CryptoDashboard.Helpers;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CryptoDashboard.Services;


namespace CryptoDashboard.ViewModels
{
    public class CoinDetailsViewModel : BaseViewModel
    {
        private readonly CoinGeckoService _coinService;
       private CancellationTokenSource? _historyCts;

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

        public Coin Coin { get; }
public string Name => Coin.Name;
public string Symbol => Coin.Symbol.ToUpper();

public decimal CurrentPrice => Coin.CurrentPrice;
public decimal PriceChange24h => Coin.PriceChangePercentage24h;
public decimal MarketCap => Coin.MarketCap;
public decimal TotalVolume => Coin.TotalVolume;

public string ImageUrl => Coin.ImageUrl;

        public PlotModel PlotModel { get; }

public async Task LoadHistoryAsync(int days = 30)
{
    try
    {
        _historyCts?.Cancel();
        _historyCts = new CancellationTokenSource();

        var history = await _coinService
            .GetMarketChartAsync(Coin.Id, days, "usd", _historyCts.Token);

        await App.Current.Dispatcher.InvokeAsync(() =>
        {
            PlotModel.Series.Clear();

            var series = new LineSeries
            {
                Title = Coin.Name,
                Color = OxyColors.SteelBlue,
                StrokeThickness = 2
            };

            foreach (var point in history)
            {
                series.Points.Add(DateTimeAxis.CreateDataPoint(
                    point.Date, (double)point.Price));
            }

            PlotModel.Series.Add(series);
            PlotModel.InvalidatePlot(true);
        });

        OnPropertyChanged(nameof(CurrentPrice));
        OnPropertyChanged(nameof(PriceChange24h));
        OnPropertyChanged(nameof(MarketCap));
        OnPropertyChanged(nameof(TotalVolume));
    }
    catch (OperationCanceledException)
    {
        // normal
    }
    catch (Exception ex)
    {
        Logger.Log("CoinDetails history failed: " + ex);
    }
}

    }
}
