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
        private IList<Action<FundDepot>> fundAddedCallbacks;
        private IList<Action<Transaction>> transactionAddedCallbacks;
        private string fundid;

        public WcfDataService()
        {
            client = new WallstreetDataServiceClient(new InstanceContext(this));
            client.SubscribeOnNewShareInformationAvailable();
            client.SubscribeOnNewOrderAvailable();
            client.SubscribeOnNewTransactionAvailable();
            client.SubscribeOnNewInvestorDepotAvailable();
            client.SubscribeOnNewFundDepotAvailable();
            marketCallbacks = new List<Action<ShareInformation>>();
            orderAddedCallbacks = new List<Action<Order>>();
            fundAddedCallbacks = new List<Action<FundDepot>>();
            transactionAddedCallbacks = new List<Action<Transaction>>();
        }

        public void Login(FundRegistration r)
        {
            fundid = r.Id;
            client.LoginFund(r);
        }

        public void PlaceOrder(Order order)
        {
            client.PutOrder(order);
        }

        public void CancelOrder(Order order)
        {
            client.DeleteOrder(order);
        }

        public FundDepot LoadFundInformation()
        {
            if (fundid != null)
            {
                return client.GetFundDepot(fundid);
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<ShareInformation> LoadMarketInformation()
        {
            return client.GetMarketInformation().Where(x => !x.IsFund);
        }

        public IEnumerable<Order> LoadPendingOrders()
        {
            if (fundid != null)
            {
                return client.GetPendingOrders(fundid);
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

        }

        public void OnNewFundDepotAvailable(FundDepot depot)
        {
            if (fundid != null && depot != null && depot.FundID.Equals(fundid))
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
