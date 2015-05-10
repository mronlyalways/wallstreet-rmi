using System;
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
        DataStore data;

        public WallstreetDataService(DataStore data)
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
            CallWithArgument(data.ShareInformationCallbacks, info);
        }

        public IEnumerable<Order> GetOrders()
        {
            return data.Orders;
        }

        public void PutOrder(Order order)
        {
            data.Orders.Add(order);
            CallWithArgument(data.OrderCallbacks, order);
        }

        public IEnumerable<Transaction> GetTransactions()
        {
            return data.Transactions;
        }

        public void PutTransaction(Transaction transaction)
        {
            data.Transactions.Add(transaction);
            CallWithArgument(data.TransactionCallbacks, transaction);
        }

        public FirmDepot RegisterFirm(Request request)
        {
            if (data.Brokers.Count > 0)
            {
                var result = data.Brokers[0](request);
                var depot = new FirmDepot { FirmName = result.Item1, OwnedShares = result.Item2 };
                var info = new ShareInformation
                {
                    FirmName = result.Item1,
                    NoOfShares = result.Item3,
                    PricePerShare = result.Item4,
                    PurchasingVolume = result.Item5,
                    SalesVolume = result.Item6
                };
                var order = new Order
                {
                    Id = result.Item7.Item1,
                    ShareName = result.Item1,
                    InvestorId = result.Item1,
                    Type = Order.OrderType.SELL,
                    Status = Order.OrderStatus.OPEN,
                    NoOfProcessedShares = 0,
                    TotalNoOfShares = result.Item7.Item2,
                    Limit = 0
                };

                data.FirmDepots = data.FirmDepots.Where(x => x.FirmName != depot.FirmName).ToList();
                data.FirmDepots.Add(depot);
                data.Orders.Add(order);
                data.ShareInformation = data.ShareInformation.Where(x => x.FirmName != info.FirmName).ToList();
                data.ShareInformation.Add(info);
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
            data.Brokers.Add(subscriber.OnNewRegistrationRequestAvailable);
        }

        private void CallWithArgument<T>(IEnumerable<Action<T>> callbacks, T arg)
        {
            foreach (Action<T> callback in callbacks)
            {
                callback(arg);
            }
        }
    }
}
