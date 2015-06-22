using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Investor.Model
{
    public class OwningShareDTO
    {
        public string ExchangeName { get; set; }

        public string ShareName { get; set; }

        public int Amount { get; set; }

        public double StockPrice { get; set; }

        public bool IsFund { get; set; }

        public double Value
        {
            get
            {
                return StockPrice * Amount;
            }
        }
    }
}
