using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.ServiceModel;
using FundManager.localhost;

namespace FundManager.Model
{
    public class WcfDataService : IDataService, IDisposable, IWallstreetDataServiceCallback
    {
        private WallstreetDataServiceClient client;
        private IList<Action<ShareInformation>> marketCallbacks;
        private IList<Action<Order>> orderAddedCallbacks;
        private IList<Action<InvestorDepot>> investorAddedCallbacks;
        private IList<Action<Transaction>> transactionAddedCallbacks;
        private IList<Action<FundDepot>> fundAddedCallbacks;
        private string fundid;

        public WcfDataService()
        {
            client = new WallstreetDataServiceClient(new InstanceContext(this));
            marketCallbacks = new List<Action<ShareInformation>>();
            orderAddedCallbacks = new List<Action<Order>>();
            fundAddedCallbacks = new List<Action<FundDepot>>();
            investorAddedCallbacks = new List<Action<InvestorDepot>>();
            transactionAddedCallbacks = new List<Action<Transaction>>();
        }

        public void Login(FundRegistration r, string exchangeId)
        {
            fundid = r.Id;
            client.SubscribeOnNewShareInformationAvailable(exchangeId);
            client.SubscribeOnNewOrderAvailable(exchangeId);
            client.SubscribeOnNewTransactionAvailable(exchangeId);
            client.SubscribeOnNewInvestorDepotAvailable(exchangeId);
            client.SubscribeOnNewFundDepotAvailable(exchangeId);
            client.LoginFund(r, exchangeId);
        }

        public void PlaceOrder(Order order, string exchangeId)
        {
            client.PutOrder(order, exchangeId);
        }

        public void CancelOrder(Order order, string exchangeId)
        {
            client.DeleteOrder(order, exchangeId);
        }

        public FundDepot LoadFundInformation(string exchangeId)
        {
            if (fundid != null)
            {
                return client.GetFundDepot(fundid, exchangeId);
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<ShareInformation> LoadMarketInformation(string exchangeId)
        {
            return client.GetMarketInformation(exchangeId).Where(x => !x.IsFund);
        }

        public IEnumerable<string> LoadExchangeInformation()
        {
            return client.GetExchanges();
        }

        public IEnumerable<Order> LoadPendingOrders(string exchangeId)
        {
            if (fundid != null)
            {
                return client.GetPendingOrders(fundid, exchangeId);
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

        public void AddNewFundInformationAvailableCallback(Action<FundDepot> callback)
        {
            fundAddedCallbacks.Add(callback);
        }

        public void RemoveNewFundInformationAvailableCallback(Action<FundDepot> callback)
        {
            fundAddedCallbacks.Remove(callback);
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
            if (!info.IsFund)
            {
                ExecuteOnGUIThread(marketCallbacks, info);
            }
        }

        public void OnNewOrderAvailable(Order order)
        {
            if (fundid != null && order.InvestorId.Equals(fundid))
            {
                ExecuteOnGUIThread(orderAddedCallbacks, order);
            }
        }

        public void OnNewInvestorDepotAvailable(InvestorDepot depot)
        {
            if (fundid != null && depot != null && depot.Id.Equals(fundid))
            {
                ExecuteOnGUIThread(investorAddedCallbacks, depot);
            }
        }

        public void OnNewFundDepotAvailable(FundDepot depot)
        {
            if (fundid != null && depot != null && depot.Id.Equals(fundid))
            {
                ExecuteOnGUIThread(fundAddedCallbacks, depot);
            }
        }

        public void OnNewTransactionAvailable(Transaction transaction)
        {
            if (fundid != null && (transaction.BuyerId.Equals(fundid) || transaction.SellerId.Equals(fundid)))
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
