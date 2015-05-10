using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WallstreetDataService.Model;

namespace WallstreetDataService
{
    public class DataStore
    {
        public DataStore()
        {
            Brokers = new List<Func<Request, Tuple<string, int, int, double, int, int, Tuple<string, int>>>>();
            ShareInformation = new List<ShareInformation>();
            Orders = new List<Order>();
            Transactions = new List<Transaction>();
            FirmDepots = new List<FirmDepot>();
            PendingRequests = new List<Request>();
            ShareInformationCallbacks = new List<Action<ShareInformation>>();
            OrderCallbacks = new List<Action<Order>>();
            TransactionCallbacks = new List<Action<Transaction>>();
        }

        public IList<Func<Request, Tuple<string, int, int, double, int, int, Tuple<string, int>>>> Brokers { get; set; }
        public IList<ShareInformation> ShareInformation { get; set; }
        public IList<Order> Orders { get; set; }
        public IList<Transaction> Transactions { get; set; }
        public IList<FirmDepot> FirmDepots { get; set; }
        public IList<Request> PendingRequests { get; set; }
        public List<Action<ShareInformation>> ShareInformationCallbacks { get; set; }
        public List<Action<Order>> OrderCallbacks { get; set; }
        public List<Action<Transaction>> TransactionCallbacks { get; set; }
    }
}
