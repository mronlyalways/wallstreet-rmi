using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using WallstreetDataService.Model;
using System.Threading;

namespace WallstreetDataService
{
    public class WallstreetDataService : IWallstreetDataService, IBrokerService
    {
        private DataRepository data;
        static private long putOrdersCounter = 0;

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
            foreach (Order o in data.Orders.Values.Where(x => x.Status != Order.OrderStatus.DONE && x.Status != Order.OrderStatus.DELETED && x.Type == Order.OrderType.BUY))
            {
                PutOrder(o);
            }
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
            NotifySubscribers(data.InvestorCallbacks, investor);
        }

        public InvestorDepot LoginInvestor(InvestorRegistration registration)
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

        public FundDepot GetFundDepot(string fundId)
        {
            FundDepot result;
            data.FundDepots.TryGetValue(fundId, out result);
            return result;
        }

        public void LoginFund(FundRegistration registration)
        {
            FundDepot depot;
            var exists = data.FundDepots.TryGetValue(registration.Id, out depot);
            if (!exists)
            {
                if (data.Brokers.Count > 0)
                {
                    FundRequestResult result = data.Brokers.First().OnNewFundRegistrationRequestAvailable(registration);
                    depot = result.FundDepot;
                    var info = result.ShareInformation;
                    var order = result.Order;
                    data.FundDepots[depot.FundID] = depot;
                    data.Orders[order.Id] = order;
                    NotifySubscribers(data.OrderCallbacks, order);
                    data.ShareInformation[info.FirmName] = info;
                    NotifySubscribers(data.ShareInformationCallbacks, info);
                    NotifySubscribers(data.FundRegistrationCallbacks, result.FundDepot);
                }
                else
                {
                    data.PendingFundRegistrationRequests.Add(registration);
                    // TODO implement mechanism to call brokers when coming online.
                }
            }
            else
            {
                NotifySubscribers(data.FundRegistrationCallbacks, data.FundDepots[registration.Id]);
            }
        }

        public IEnumerable<Order> GetOrders()
        {
            return data.Orders.Values;
        }

        public IEnumerable<Order> GetPendingOrders(string investorId)
        {
            return data.Orders.Values.Where(x => x.InvestorId.Equals(investorId) && (x.Status == Order.OrderStatus.PARTIAL || x.Status == Order.OrderStatus.OPEN));
        }

        public void PutOrder(Order order)
        {
            data.Orders[order.Id] = order;

            if (data.Brokers.Count > 0)
            {
                Interlocked.Increment(ref putOrdersCounter);
                int counter = (int) Interlocked.Read(ref putOrdersCounter) % data.Brokers.Count;

                IBroker broker = data.Brokers.ToList()[counter];

                if (counter == data.Brokers.Count)
                {
                    Interlocked.Exchange(ref putOrdersCounter, 0);
                }
                
                var prio_result = broker.OnNewOrderMatchingRequestAvailable(order, data.Orders.Values.Where(x => x.ShareName.Equals(order.ShareName) && x.Type != order.Type && x.Prioritize && x.Status != Order.OrderStatus.DONE && x.Status != Order.OrderStatus.DELETED));
                ProcessOrder(order, prio_result);

                var result = broker.OnNewOrderMatchingRequestAvailable(order, data.Orders.Values.Where(x => x.ShareName.Equals(order.ShareName) && x.Type != order.Type && x.Status != Order.OrderStatus.DONE && x.Status != Order.OrderStatus.DELETED));
                ProcessOrder(order, result);
            }
        }

        private void ProcessOrder(Order order, OrderMatchResult result)
        {
            if (result.Order != null) // else punish : do nothing
            {
                foreach (Order o in result.Matches)
                {
                    data.Orders[o.Id] = o;
                }

                foreach (Transaction t in result.Transactions)
                {
                    var buyer = data.InvestorDepots[t.BuyerId];
                    buyer.Budget -= (t.TotalCost + t.BuyerProvision);
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
                    NotifySubscribers(data.InvestorCallbacks, buyer);
                    NotifySubscribers(data.InvestorCallbacks, seller);
                }
                NotifySubscribers(data.OrderCallbacks, result.Order);
                data.Orders[order.Id] = result.Order;
                foreach (Order ord in result.Matches)
                {
                    NotifySubscribers(data.OrderCallbacks, ord);
                }

                var info = data.ShareInformation[order.ShareName];
                info.PurchasingVolume = CalculatePurchasingVolume(data.Orders.Values);
                info.SalesVolume = CalculateSalesVolume(data.Orders.Values);
                NotifySubscribers(data.ShareInformationCallbacks, info);
            }
            else
            {
                data.Orders.TryRemove(order.Id, out order);
            }
        }

        public void DeleteOrder(Order order)
        {
            Order o;
            data.Orders.TryRemove(order.Id, out o);
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

        public FirmDepot GetFirmDepot(string firmName)
        {
            FirmDepot result;
            data.FirmDepots.TryGetValue(firmName, out result);
            return result;
        }

        public FirmDepot RegisterFirm(FirmRegistration request)
        {
            if (data.Brokers.Count > 0)
            {
                FirmRequestResult result = data.Brokers.First().OnNewFirmRegistrationRequestAvailable(request) as FirmRequestResult;
                var depot = result.FirmDepot;
                var info = result.ShareInformation;
                var order = result.Order;
                data.FirmDepots[depot.FirmName] = depot;
                data.Orders[order.Id] = order;
                NotifySubscribers(data.OrderCallbacks, order);
                data.ShareInformation[info.FirmName] = info;
                NotifySubscribers(data.ShareInformationCallbacks, info);
                return result.FirmDepot;
            }
            else
            {
                data.PendingFirmRegistrationRequests.Add(request);
                // TODO implement mechanism to call brokers when coming online.
                return null;
            }
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

        public void SubscribeOnNewFundDepotAvailable()
        {
            var subscriber = OperationContext.Current.GetCallbackChannel<IWallstreetSubscriber>();
            data.FundRegistrationCallbacks.Add(subscriber.OnNewFundDepotAvailable);
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

        public void SubscribeOnNewInvestorDepotAvailable()
        {
            var subscriber = OperationContext.Current.GetCallbackChannel<IWallstreetSubscriber>();
            data.InvestorCallbacks.Add(subscriber.OnNewInvestorDepotAvailable);
        }

        public void RegisterBroker()
        {
            bool empty = data.Brokers.Count == 0;
            var subscriber = OperationContext.Current.GetCallbackChannel<IBroker>();
            data.Brokers.Add(subscriber);

            if (empty && data.Brokers.Count == 1)
            {
                Order[] pending = data.Orders.Values.Where(x => x.Status != Order.OrderStatus.DONE && x.Status != Order.OrderStatus.DELETED).ToArray();

                foreach (Order o in pending)
                {
                    PutOrder(o);
                }
                //TODO pending orders
            }
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

        private int CalculateSalesVolume(IEnumerable<Order> orders)
        {
            return orders.Where(x => x.Status != Order.OrderStatus.DONE && x.Type == Order.OrderType.SELL).Sum(x => x.NoOfOpenShares);
        }

        private int CalculatePurchasingVolume(IEnumerable<Order> orders)
        {
            return orders.Where(x => x.Status != Order.OrderStatus.DONE && x.Type == Order.OrderType.BUY).Sum(x => x.NoOfOpenShares);
        }
    }
}
