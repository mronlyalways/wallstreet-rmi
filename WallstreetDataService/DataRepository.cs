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
            ShareInformation = new ConcurrentBag<ShareInformation>();
            InvestorDepots = new ConcurrentBag<InvestorDepot>();
            PendingOrders = new ConcurrentBag<Order>();
            Transactions = new ConcurrentBag<Transaction>();
            FirmDepots = new ConcurrentBag<FirmDepot>();
            PendingRequests = new ConcurrentBag<Request>();
            ShareInformationCallbacks = new ConcurrentBag<Action<ShareInformation>>();
            OrderCallbacks = new ConcurrentBag<Action<Order>>();
            TransactionCallbacks = new ConcurrentBag<Action<Transaction>>();
        }

        public ConcurrentBag<IBroker> Brokers { get; set; }
        public ConcurrentBag<ShareInformation> ShareInformation { get; set; }
        public ConcurrentBag<InvestorDepot> InvestorDepots { get; set; }
        public ConcurrentBag<Order> PendingOrders { get; set; }
        public ConcurrentBag<Transaction> Transactions { get; set; }
        public ConcurrentBag<FirmDepot> FirmDepots { get; set; }
        public ConcurrentBag<Request> PendingRequests { get; set; }
        public ConcurrentBag<Action<ShareInformation>> ShareInformationCallbacks { get; set; }
        public ConcurrentBag<Action<Order>> OrderCallbacks { get; set; }
        public ConcurrentBag<Action<Transaction>> TransactionCallbacks { get; set; }
    }
}
