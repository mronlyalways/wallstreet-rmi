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
        private IList<Action<InvestorDepot>> investorAddedCallbacks;
        private IList<Action<Transaction>> transactionAddedCallbacks;
        private string email;

        public WcfDataService()
        {
            client = new WallstreetDataServiceClient(new InstanceContext(this));
            marketCallbacks = new List<Action<ShareInformation>>();
            orderAddedCallbacks = new List<Action<Order>>();
            investorAddedCallbacks = new List<Action<InvestorDepot>>();
            transactionAddedCallbacks = new List<Action<Transaction>>();
        }

        public InvestorDepot Login(InvestorRegistration r, string exchangeId)
        {
            email = r.Email;
            client.SubscribeOnNewShareInformationAvailable(exchangeId);
            client.SubscribeOnNewOrderAvailable(exchangeId);
            client.SubscribeOnNewTransactionAvailable(exchangeId);
            client.SubscribeOnNewInvestorDepotAvailable(exchangeId);
            return client.LoginInvestor(r, exchangeId);
        }

        public void PlaceOrder(Order order, string exchangeId)
        {
            client.PutOrder(order, exchangeId);
        }

        public void CancelOrder(Order order, string exchangeId)
        {
            client.DeleteOrder(order, exchangeId);
        }

        public InvestorDepot LoadInvestorInformation(string exchangeId)
        {
            if (email != null)
            {
                return client.GetInvestorDepot(email, exchangeId);
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<string> LoadExchangeInformation()
        {
            return client.GetExchanges();
        }

        public IEnumerable<ShareInformation> LoadMarketInformation(string exchangeId)
        {
            return client.GetMarketInformation(exchangeId);
        }

        public IEnumerable<Order> LoadPendingOrders(string exchangeId)
        {
            if (email != null)
            {
                return client.GetPendingOrders(email, exchangeId);
            }
            else
            {
                return new List<Order>();
            }
        }

        public void AddNewMarketInformationAvailableCallback(Action<ShareInformation> callback)
        {
            marketCallbacks.Add(callback);
        }

        public void AddNewInvestorInformationAvailableCallback(Action<InvestorDepot> callback)
        {
            investorAddedCallbacks.Add(callback);
        }

        public void AddNewOrderAvailableCallback(Action<Order> callback)
        {
            orderAddedCallbacks.Add(callback);
        }

        public void OnNewShareInformationAvailable(ShareInformation info)
        {
            ExecuteOnGUIThread(marketCallbacks, info);
        }

        public void OnNewOrderAvailable(Order order)
        {
            if (email != null && order.InvestorId.Equals(email))
            {
                ExecuteOnGUIThread(orderAddedCallbacks, order);
            }
        }

        public void OnNewInvestorDepotAvailable(InvestorDepot depot)
        {
            if (email != null && depot != null && depot.Id.Equals(email))
            {
                ExecuteOnGUIThread(investorAddedCallbacks, depot);
            }
        }

        public void OnNewFundDepotAvailable(FundDepot depot) { }

        public void OnNewTransactionAvailable(Transaction transaction)
        {
            if (email != null && (transaction.BuyerId.Equals(email) || transaction.SellerId.Equals(email)))
            {
                ExecuteOnGUIThread(transactionAddedCallbacks, transaction);
            }
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
