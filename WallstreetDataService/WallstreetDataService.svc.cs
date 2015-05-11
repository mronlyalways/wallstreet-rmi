using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using WallstreetDataService.Model;

namespace WallstreetDataService
{
    public class WallstreetDataService : IWallstreetDataService, IBrokerService
    {
        DataRepository data;

        public WallstreetDataService(DataRepository data)
        {
            this.data = data;
        }

        public IEnumerable<ShareInformation> GetMarketInformation()
        {
            return data.ShareInformation;
        }

        public ShareInformation GetShareInformation(string shareName)
        {
            return data.ShareInformation.SingleOrDefault(x => x.FirmName == shareName);
        }

        public void PutShareInformation(ShareInformation info)
        {
            data.ShareInformation.Add(info);
            NotifySubscribers(data.ShareInformationCallbacks, info);
        }

        public IEnumerable<InvestorDepot> GetInvestorInformation()
        {
            return data.InvestorDepots;
        }

        public InvestorDepot GetInvestorDepot(string investorId)
        {
            return data.InvestorDepots.SingleOrDefault(x => x.Email == investorId);
        }

        public void PutInvestorDepot(InvestorDepot investor)
        {
            data.InvestorDepots.Add(investor);
        }

        public IEnumerable<Order> GetOrders()
        {
            return data.Orders;
        }

        public void PutOrder(Order order)
        {
            if (data.Brokers.Count > 0)
            {
                var result = data.Brokers.First().OnNewOrderMatchingRequestAvailable(order, data.Orders.Where(x => x.ShareName == order.ShareName));
            }
            else
            {
                //data.PendingRequests.Add(request);
                // TODO implement mechanism to call brokers when coming online.
                //return null;
            }
            data.Orders.Add(order);
            NotifySubscribers(data.OrderCallbacks, order);
        }

        public IEnumerable<Transaction> GetTransactions()
        {
            return data.Transactions;
        }

        public void PutTransaction(Transaction transaction)
        {
            data.Transactions.Add(transaction);
            NotifySubscribers(data.TransactionCallbacks, transaction);
        }

        public FirmDepot RegisterFirm(Request request)
        {
            if (data.Brokers.Count > 0)
            {
                var result = data.Brokers.First().OnNewRegistrationRequestAvailable(request);
                var depot = result.FirmDepot;
                var info = result.ShareInformation;
                var order = result.Order;
                data.FirmDepots = new ConcurrentBag<FirmDepot>(data.FirmDepots.Where(x => x.FirmName != depot.FirmName));
                data.FirmDepots.Add(depot);
                data.Orders.Add(order);
                NotifySubscribers(data.OrderCallbacks, order);
                data.ShareInformation = new ConcurrentBag<ShareInformation>(data.ShareInformation.Where(x => x.FirmName != info.FirmName));
                data.ShareInformation.Add(info);
                NotifySubscribers(data.ShareInformationCallbacks, info);
                return depot;
            }
            else
            {
                data.PendingRequests.Add(request);
                // TODO implement mechanism to call brokers when coming online.
                return null;
            }
        }

        public FirmDepot GetFirmDepot(string firmName)
        {
            return data.FirmDepots.SingleOrDefault(x => x.FirmName.Equals(firmName));
        }

        public void SubscribeOnNewRegistrationRequestAvailable()
        {
            var subscriber = OperationContext.Current.GetCallbackChannel<IWallstreetSubscriber>();
            data.ShareInformationCallbacks.Add(subscriber.OnNewShareInformationAvailable);
        }

        public void SubscribeOnNewShareInformationAvailable()
        {
            var subscriber = OperationContext.Current.GetCallbackChannel<IWallstreetSubscriber>();
            data.ShareInformationCallbacks.Add(subscriber.OnNewShareInformationAvailable);
        }

        public void SubscribeOnNewOrderAvailable()
        {
            var subscriber = OperationContext.Current.GetCallbackChannel<IWallstreetSubscriber>();
            data.OrderCallbacks.Add(subscriber.OnNewOrderAvailable);
        }

        public void SubscribeOnNewTransactionAvailable()
        {
            var subscriber = OperationContext.Current.GetCallbackChannel<IWallstreetSubscriber>();
            data.TransactionCallbacks.Add(subscriber.OnNewTransactionAvailable);
        }

        public void RegisterBroker()
        {
            var subscriber = OperationContext.Current.GetCallbackChannel<IBroker>();
            data.Brokers.Add(subscriber);
        }

        private void NotifySubscribers<T>(IEnumerable<Action<T>> callbacks, T arg)
        {
            foreach (Action<T> callback in callbacks)
            {
                callback(arg);
            }
        }
    }
}
