using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallstreet.localhost;

namespace Wallstreet.Model
{
    public interface IDataService
    {
        IEnumerable<string> LoadExchanges();

        IEnumerable<ShareInformation> LoadMarketInformation();

        IEnumerable<Order> LoadOrders();

        IEnumerable<Transaction> LoadTransactions();

        void AddNewMarketInformationAvailableCallback(Action<ShareInformation> callback);

        void AddNewOrderAddedCallback(Action<Order> callback);

        void AddOrderRemovedCallback(Action<Order> callback);

        void AddNewTransactionAddedCallback(Action<Transaction> callback);
    }
}
