using System;

namespace CryptoDashboard.Models
{
    public class PricePoint
    {
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
        public decimal Volume { get; set; }
    }
}
