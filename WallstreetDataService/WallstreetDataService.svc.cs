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
        private DataRepository data;

        public WallstreetDataService(DataRepository data)
        {
            this.data = data;
        }

        public IEnumerable<ShareInformation> GetMarketInformation()
        {
            return data.ShareInformation.Values;
        }

        public ShareInformation GetShareInformation(string shareName)
        {
            ShareInformation result;
            data.ShareInformation.TryGetValue(shareName, out result);
            return result;
        }

        public void PutShareInformation(ShareInformation info)
        {
            data.ShareInformation[info.FirmName] = info;
            NotifySubscribers(data.ShareInformationCallbacks, info);
        }

        public IEnumerable<InvestorDepot> GetInvestorInformation()
        {
            return data.InvestorDepots.Values;
        }

        public InvestorDepot GetInvestorDepot(string investorId)
        {
            InvestorDepot result;
            data.InvestorDepots.TryGetValue(investorId, out result);
            return result;
        }

        public void PutInvestorDepot(InvestorDepot investor)
        {
            data.InvestorDepots[investor.Email] = investor;
        }

        public InvestorDepot LoginInvestor(Registration registration)
        {
            InvestorDepot depot;
            var exists = data.InvestorDepots.TryGetValue(registration.Email, out depot);
            if (!exists)
            {
                depot = new InvestorDepot { Email = registration.Email, Budget = 0, Shares = new Dictionary<string, int>() };
            }
            depot.Budget += registration.Budget;
            data.InvestorDepots[depot.Email] = depot;
            return depot;
        }

        public IEnumerable<Order> GetOrders()
        {
            return data.PendingOrders.Values;
        }

        public void PutOrder(Order order)
        {
            if (data.Brokers.Count > 0)
            {
                var result = data.Brokers.First().OnNewOrderMatchingRequestAvailable(order, data.PendingOrders.Values.Where(x => x.ShareName == order.ShareName && x.Type != order.Type));
                if (result == null)
                {
                    data.PendingOrders[order.Id] = order;
                }
                else
                {
                    foreach (Order o in result.Matches)
                    {
                        if (o.Status != Order.OrderStatus.DONE)
                        {
                            data.PendingOrders[o.Id] = o;
                        }
                    }

                    foreach (Transaction t in result.Transactions)
                    {
                        var buyer = data.InvestorDepots[t.BuyerId];
                        buyer.Budget -= (t.TotalCost + t.Provision);
                        int val;
                        buyer.Shares[order.ShareName] = buyer.Shares.TryGetValue(order.ShareName, out val) ? val + t.NoOfSharesSold : t.NoOfSharesSold;
                        InvestorDepot seller;
                        var sellerExists = data.InvestorDepots.TryGetValue(t.SellerId, out seller);
                        if (sellerExists) // seller is investor
                        {
                            seller.Budget += t.TotalCost;
                            seller.Shares[order.ShareName] -= t.NoOfSharesSold;
                        }
                        else // seller is firm
                        {
                            var firm = data.FirmDepots[t.SellerId];
                            firm.OwnedShares -= t.NoOfSharesSold;
                        }
                        data.Transactions.Add(t);
                        NotifySubscribers(data.TransactionCallbacks, t);
                    }
                    NotifySubscribers(data.OrderCallbacks, result.Order);
                    foreach (Order ord in result.Matches)
                    {
                        NotifySubscribers(data.OrderCallbacks, ord);
                    }
                }
            }
            else
            {
                data.PendingOrders[order.Id] = order;
            }
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
                FirmRequestResult result = data.Brokers.First().OnNewRegistrationRequestAvailable(request);
                var depot = result.FirmDepot;
                var info = result.ShareInformation;
                var order = result.Order;
                data.FirmDepots[depot.FirmName] = depot;
                data.PendingOrders[order.Id] = order;
                NotifySubscribers(data.OrderCallbacks, order);
                data.ShareInformation[info.FirmName] = info;
                NotifySubscribers(data.ShareInformationCallbacks, info);
                return result.FirmDepot;
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
            FirmDepot result;
            data.FirmDepots.TryGetValue(firmName, out result);
            return result;
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

        public void UnregisterBroker()
        {
            var subscriber = OperationContext.Current.GetCallbackChannel<IBroker>();
            data.Brokers = new ConcurrentBag<IBroker>(data.Brokers.Where(x => x.GetHashCode() != subscriber.GetHashCode()));
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
