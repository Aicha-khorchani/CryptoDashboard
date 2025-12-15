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
