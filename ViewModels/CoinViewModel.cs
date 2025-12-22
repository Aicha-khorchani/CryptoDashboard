using CryptoDashboard.Models;

namespace CryptoDashboard.ViewModels
{
    public class CoinViewModel : BaseViewModel
    {
        private readonly Coin _coin;

        public CoinViewModel(Coin coin)
        {
            _coin = coin;
            _isFavorite = coin.IsFavorite; 
        }

        public string Name => _coin.Name;
        public decimal CurrentPrice => _coin.CurrentPrice;
        public string Id => _coin.Id;
        public string ImageUrl => _coin.ImageUrl;
private decimal? _alertPrice;
public decimal? AlertPrice
{
    get => _alertPrice;
    set => SetProperty(ref _alertPrice, value);
}

private bool _alertTriggered;
public bool AlertTriggered
{
    get => _alertTriggered;
    set => SetProperty(ref _alertTriggered, value);
}

        private bool _isFavorite;
        public bool IsFavorite
        {
            get => _isFavorite;
            set
            {
                if (SetProperty(ref _isFavorite, value))
                {
                    _coin.IsFavorite = value; 
                }
            }
        }

        public Coin GetModel() => _coin;
    }
}
