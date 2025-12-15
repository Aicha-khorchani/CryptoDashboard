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

        public PlotModel PlotModel { get; }

        public async Task LoadHistoryAsync(int days = 30)
        {
            var history = await _coinService.GetMarketChartAsync(Coin.Id, days);

            PlotModel.Series.Clear();

            var series = new LineSeries
            {
                Title = Coin.Name,
                Color = OxyColors.SteelBlue,
                StrokeThickness = 2
            };

            foreach (var point in history)
            {
                series.Points.Add(DateTimeAxis.CreateDataPoint(point.Date, (double)point.Price));
            }

            PlotModel.Series.Add(series);
            PlotModel.InvalidatePlot(true);
        }
    }
}
