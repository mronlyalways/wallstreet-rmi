using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wallstreet.localhost;
using System.ServiceModel;

namespace Wallstreet.Model
{
    public class WcfDataService : IDataService, IDisposable, IWallstreetDataServiceCallback
    {
        private IEnumerable<string> exchanges;
        private WallstreetDataServiceClient client;
        private IList<Action<ShareInformation>> marketCallbacks;
        private IList<Action<Order>> orderAddedCallbacks;
        private IList<Action<Order>> orderRemovedCallbacks;
        private IList<Action<Transaction>> transactionAddedCallbacks;

        public WcfDataService()
        {
            client = new WallstreetDataServiceClient(new InstanceContext(this));
            exchanges = client.GetExchanges();
            foreach (string e in exchanges)
            {
                client.SubscribeOnNewShareInformationAvailable(e);
                client.SubscribeOnNewOrderAvailable(e);
                client.SubscribeOnNewTransactionAvailable(e);
            }
            marketCallbacks = new List<Action<ShareInformation>>();
            orderAddedCallbacks = new List<Action<Order>>();
            orderRemovedCallbacks = new List<Action<Order>>();
            transactionAddedCallbacks = new List<Action<Transaction>>();
        }

        public IEnumerable<string> LoadExchanges()
        {
            return exchanges;
        }

        public IEnumerable<ShareInformation> LoadMarketInformation()
        {
            return GenericLoad<ShareInformation>(exchanges, client.GetMarketInformation);
        }

        public IEnumerable<Order> LoadOrders()
        {
            return GenericLoad<Order>(exchanges, client.GetOrders);
        }

        public IEnumerable<Transaction> LoadTransactions()
        {
            return GenericLoad<Transaction>(exchanges, client.GetTransactions);
        }

        private IEnumerable<T> GenericLoad<T>(IEnumerable<string> exchanges, Func<string, IEnumerable<T>> function)
        {
            var list = new List<T>();
            foreach (string e in exchanges)
            {
                list.AddRange(function(e));
            }
            return list;
        }

        public void AddNewMarketInformationAvailableCallback(Action<ShareInformation> callback)
        {
            marketCallbacks.Add(callback);
        }

        public void AddNewOrderAddedCallback(Action<Order> callback)
        {
            orderAddedCallbacks.Add(callback);
        }

        public void AddOrderRemovedCallback(Action<Order> callback)
        {
            orderRemovedCallbacks.Add(callback);
        }

        public void AddNewTransactionAddedCallback(Action<Transaction> callback)
        {
            transactionAddedCallbacks.Add(callback);
        }

        public void OnNewInvestorDepotAvailable(InvestorDepot transaction) { }

        public void OnNewFundDepotAvailable(FundDepot depot) { }

        public void OnNewShareInformationAvailable(ShareInformation info)
        {
            ExecuteOnGUIThread(marketCallbacks, info);
        }

        public void OnNewOrderAvailable(Order order)
        {
            ExecuteOnGUIThread(orderAddedCallbacks, order);
        }

        public void OnNewTransactionAvailable(Transaction transaction)
        {
            ExecuteOnGUIThread(transactionAddedCallbacks, transaction);
        }

        private void OnOrderEntryRemoved(Order order)
        {
            ExecuteOnGUIThread(orderRemovedCallbacks, order);
        }

        private void ExecuteOnGUIThread<T>(IEnumerable<Action<T>> callbacks, T arg)
        {
            foreach (Action<T> callback in callbacks)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    callback(arg);
                }), null);
            }
        }

        public void Dispose()
        {
            
        }
    }
}
