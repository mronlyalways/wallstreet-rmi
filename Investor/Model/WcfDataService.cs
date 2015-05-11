using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.ServiceModel;
using Investor.localhost;

namespace Investor.Model
{
    public class WcfDataService : IDataService, IDisposable, IWallstreetDataServiceCallback
    {
        private WallstreetDataServiceClient client;
        private IList<Action<ShareInformation>> marketCallbacks;
        private IList<Action<Order>> orderAddedCallbacks;
        private IList<Action<Order>> orderRemovedCallbacks;
        private IList<Action<IEnumerable<Order>>> pendingOrdersAddedCallbacks;
        private IList<Action<InvestorDepot>> investorAddedCallbacks;
        private IList<Action<Transaction>> transactionAddedCallbacks;
        private string email;

        public WcfDataService()
        {
            client = new WallstreetDataServiceClient(new InstanceContext(this));
            client.SubscribeOnNewShareInformationAvailable();
            client.SubscribeOnNewOrderAvailable();
            client.SubscribeOnNewTransactionAvailable();
            marketCallbacks = new List<Action<ShareInformation>>();
            orderAddedCallbacks = new List<Action<Order>>();
            orderRemovedCallbacks = new List<Action<Order>>();
            pendingOrdersAddedCallbacks = new List<Action<IEnumerable<Order>>>();
            investorAddedCallbacks = new List<Action<InvestorDepot>>();
            transactionAddedCallbacks = new List<Action<Transaction>>();
        }

        public InvestorDepot Login(Registration r)
        {
            email = r.Email;
            return client.LoginInvestor(r);
        }

        public void PlaceOrder(Order order)
        {
            client.PutOrder(order);
        }

        public void CancelOrder(Order order) { }

        public InvestorDepot LoadInvestorInformation()
        {
            if (email != null)
            {
                return client.GetInvestorDepot(email);
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<ShareInformation> LoadMarketInformation()
        {
            return client.GetMarketInformation();
        }

        public IEnumerable<Order> LoadPendingOrders()
        {
            return new List<Order>();
        }

        public void AddNewMarketInformationAvailableCallback(Action<ShareInformation> callback)
        {
            marketCallbacks.Add(callback);
        }

        public void AddNewPendingOrdersCallback(Action<IEnumerable<Order>> callback)
        {
            pendingOrdersAddedCallbacks.Add(callback);
        }

        public void AddNewInvestorInformationAvailableCallback(Action<InvestorDepot> callback)
        {
            investorAddedCallbacks.Add(callback);
        }

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
