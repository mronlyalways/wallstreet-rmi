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

        public InvestorDepot LoginInvestor(Registration registration)
        {
            var depot = data.InvestorDepots.SingleOrDefault(x => x.Email == registration.Email);
            if (depot == null)
            {
                depot = new InvestorDepot { Email = registration.Email, Budget = 0, Shares = new Dictionary<string, int>() };
            }
            depot.Budget += registration.Budget;
            data.InvestorDepots = new ConcurrentBag<InvestorDepot>(data.InvestorDepots.Where(x => x.Email != depot.Email).Union(new List<InvestorDepot> { depot }));
            return depot;
        }

        public IEnumerable<Order> GetOrders()
        {
            return data.PendingOrders;
        }

        public void PutOrder(Order order)
        {
            if (data.Brokers.Count > 0)
            {
                var result = data.Brokers.First().OnNewOrderMatchingRequestAvailable(order, data.PendingOrders.Where(x => x.ShareName == order.ShareName && x.Type != order.Type));
                if (result == null)
                {
                    data.PendingOrders.Add(order);
                }
                else
                {
                    data.PendingOrders = new ConcurrentBag<Order>(data.PendingOrders
                        .Where(x => !result.Matches.Select(y => y.Id).Contains(x.Id)).Union(result.Matches)
                        .Where(x => x.Id != order.Id).Union(new List<Order> {order}));

                    foreach (Transaction t in result.Transactions)
                    {
                        var buyer = data.InvestorDepots.Single(x => x.Email == t.BuyerId);
                        buyer.Budget -= (t.TotalCost + t.Provision);
                        buyer.Shares[order.ShareName] += t.NoOfSharesSold;
                        var seller = data.InvestorDepots.SingleOrDefault(x => x.Email == t.SellerId);
                        if (seller != null) // seller is investor
                        {
                            seller.Budget += t.TotalCost;
                            seller.Shares[order.ShareName] -= t.NoOfSharesSold;
                        }
                        else // seller is firm
                        {
                            var firm = data.FirmDepots.Single(x => x.FirmName == t.SellerId);
                            firm.OwnedShares -= t.NoOfSharesSold;
                        }
                        NotifySubscribers(data.TransactionCallbacks, t);
                    }
                    NotifySubscribers(data.OrderCallbacks, result.Order);
                    foreach (Order o in result.Matches)
                    {
                        NotifySubscribers(data.OrderCallbacks, o);
                    }
                }
            }
            else
            {
                data.PendingOrders.Add(order);
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

        public FirmRequestResult RegisterFirm(Request request)
        {
            if (data.Brokers.Count > 0)
            {
                FirmRequestResult result = data.Brokers.First().OnNewRegistrationRequestAvailable(request);
                var depot = result.FirmDepot;
                var info = result.ShareInformation;
                var order = result.Order;
                data.FirmDepots = new ConcurrentBag<FirmDepot>(data.FirmDepots.Where(x => x.FirmName != depot.FirmName));
                data.FirmDepots.Add(depot);
                data.PendingOrders.Add(order);
                NotifySubscribers(data.OrderCallbacks, order);
                data.ShareInformation = new ConcurrentBag<ShareInformation>(data.ShareInformation.Where(x => x.FirmName != info.FirmName));
                data.ShareInformation.Add(info);
                NotifySubscribers(data.ShareInformationCallbacks, info);
                return result;
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
