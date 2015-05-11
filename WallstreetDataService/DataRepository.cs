using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WallstreetDataService.Model;

namespace WallstreetDataService
{
    public class DataRepository
    {
        public DataRepository()
        {
            Brokers = new ConcurrentBag<IBroker>();
            ShareInformation = new ConcurrentDictionary<string, ShareInformation>();
            InvestorDepots = new ConcurrentDictionary<string, InvestorDepot>();
            Orders = new ConcurrentDictionary<string, Order>();
            Transactions = new ConcurrentBag<Transaction>();
            FirmDepots = new ConcurrentDictionary<string, FirmDepot>();
            PendingRequests = new ConcurrentBag<Request>();
            ShareInformationCallbacks = new ConcurrentBag<Action<ShareInformation>>();
            OrderCallbacks = new ConcurrentBag<Action<Order>>();
            InvestorCallbacks = new ConcurrentBag<Action<InvestorDepot>>();
            TransactionCallbacks = new ConcurrentBag<Action<Transaction>>();
        }

        public ConcurrentBag<IBroker> Brokers { get; set; }
        public ConcurrentDictionary<string, ShareInformation> ShareInformation { get; set; }
        public ConcurrentDictionary<string, InvestorDepot> InvestorDepots { get; set; }
        public ConcurrentDictionary<string, Order> Orders { get; set; }
        public ConcurrentBag<Transaction> Transactions { get; set; }
        public ConcurrentDictionary<string, FirmDepot> FirmDepots { get; set; }
        public ConcurrentBag<Request> PendingRequests { get; set; }
        public ConcurrentBag<Action<ShareInformation>> ShareInformationCallbacks { get; set; }
        public ConcurrentBag<Action<Order>> OrderCallbacks { get; set; }
        public ConcurrentBag<Action<InvestorDepot>> InvestorCallbacks { get; set; }
        public ConcurrentBag<Action<Transaction>> TransactionCallbacks { get; set; }
    }
}
