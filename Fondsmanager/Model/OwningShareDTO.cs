using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundManager.Model
{
    public class OwningShareDTO
    {
        public string ShareName { get; set; }

        public string ExchangeName { get; set; }

        public int Amount { get; set; }

        public double StockPrice { get; set; }

        public double Value {
            get
            {
                return StockPrice * Amount;
            }
        }
    }
}
