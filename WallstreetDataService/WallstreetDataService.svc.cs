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
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class WallstreetDataService : IWallstreetDataService, IBrokerService
    {
        private DataRepository data;
        static private long putOrdersCounter = 0;

        public WallstreetDataService(DataRepository data)
        {
            this.data = data;
        }

        public IEnumerable<string> GetExchanges()
        {
            return data.Exchanges.Keys;
        }

        public IEnumerable<ShareInformation> GetMarketInformation(string exchangeId)
        {
            return data.Exchanges[exchangeId].ShareInformation.Values;
        }

        public ShareInformation GetShareInformation(string shareName, string exchangeId)
        {
            ShareInformation result;
            data.Exchanges[exchangeId].ShareInformation.TryGetValue(shareName, out result);
            return result;
        }

        public void PutShareInformation(ShareInformation info, string exchangeId)
        {
            data.Exchanges[exchangeId].ShareInformation[info.FirmName] = info;
            NotifySubscribers(data.Exchanges[exchangeId].ShareInformationCallbacks, info);
            foreach (Order o in data.Exchanges[exchangeId].Orders.Values.Where(x => x.Status != Order.OrderStatus.DONE && x.Status != Order.OrderStatus.DELETED && x.Type == Order.OrderType.BUY))
            {
                PutOrder(o, exchangeId);
            }
        }

        public IEnumerable<InvestorDepot> GetInvestorInformation(string exchangeId)
        {
            return data.Exchanges[exchangeId].InvestorDepots.Values;
        }

        public InvestorDepot GetInvestorDepot(string investorId, string exchangeId)
        {
            InvestorDepot investor;
            data.Exchanges[exchangeId].InvestorDepots.TryGetValue(investorId, out investor);
            return investor;
        }

        public void PutInvestorDepot(InvestorDepot investor, string exchangeId)
        {
            data.Exchanges[exchangeId].InvestorDepots[investor.Id] = investor;
            NotifySubscribers(data.Exchanges[exchangeId].InvestorCallbacks, investor);
        }

        public InvestorDepot LoginInvestor(InvestorRegistration registration, string exchangeId)
        {
            InvestorDepot depot;
            var exists = data.Exchanges[exchangeId].InvestorDepots.TryGetValue(registration.Email, out depot);
            if (!exists)
            {
                depot = new InvestorDepot { Id = registration.Email, ExchangeName = exchangeId, Budget = 0, Shares = new Dictionary<string, int>() };
            }
            depot.Budget += registration.Budget;
            data.Exchanges[exchangeId].InvestorDepots[depot.Id] = depot;
            return depot;
        }

        public FundDepot GetFundDepot(string fundId, string exchangeId)
        {
            FundDepot result = new FundDepot();
            data.Exchanges[exchangeId].FundDepots.TryGetValue(fundId, out result);
            return result;
        }

        public void LoginFund(FundRegistration registration, string exchangeId)
        {
            FundDepot depot;
            var exists = data.Exchanges[exchangeId].FundDepots.TryGetValue(registration.Id, out depot);
            if (!exists)
            {
                if (data.Exchanges[exchangeId].Brokers.Count > 0)
                {
                    FundRequestResult result = data.Exchanges[exchangeId].Brokers.First().ProcessFundRegistration(registration);
                    depot = result.FundDepot;
                    var info = result.ShareInformation;
                    var order = result.Order;
                    data.Exchanges[exchangeId].FundDepots[depot.Id] = depot;
                    data.Exchanges[exchangeId].InvestorDepots[depot.Id] = depot as InvestorDepot;
                    if (info != null && order != null)
                    {
                        data.Exchanges[exchangeId].Orders[order.Id] = order;
                        data.Exchanges[exchangeId].ShareInformation[info.FirmName] = info;
                        NotifySubscribers(data.Exchanges[exchangeId].OrderCallbacks, order);
                        NotifySubscribers(data.Exchanges[exchangeId].ShareInformationCallbacks, info);
                    }
                    NotifySubscribers(data.Exchanges[exchangeId].FundCallbacks, result.FundDepot);
                }
                else
                {
                    data.Exchanges[exchangeId].PendingFundRegistrationRequests.Add(registration);
                }
            }
            else
            {
                NotifySubscribers(data.Exchanges[exchangeId].FundCallbacks, data.Exchanges[exchangeId].FundDepots[registration.Id]);
            }
        }

        public IEnumerable<string> GetRegisteredExchanges(string investorId)
        {
            var list = new List<string>();
            foreach (string e in data.Exchanges.Keys)
            {
                if (data.Exchanges[e].InvestorDepots.Keys.Contains(investorId))
                {
                    list.Add(e);
                }
            }
            return list;
        }

        public IEnumerable<Order> GetOrders(string exchangeId)
        {
            return data.Exchanges[exchangeId].Orders.Values;
        }

        public IEnumerable<Order> GetPendingOrders(string investorId, string exchangeId)
        {
            return data.Exchanges[exchangeId].Orders.Values.Where(x => x.InvestorId.Equals(investorId) && (x.Status == Order.OrderStatus.PARTIAL || x.Status == Order.OrderStatus.OPEN));
        }

        public void PutOrder(Order order, string exchangeId)
        {
            data.Exchanges[exchangeId].Orders[order.Id] = order;

            if (data.Exchanges[exchangeId].Brokers.Count > 0)
            {
                Interlocked.Increment(ref putOrdersCounter);
                int counter = (int)Interlocked.Read(ref putOrdersCounter) % data.Exchanges[exchangeId].Brokers.Count;

                IBroker broker = data.Exchanges[exchangeId].Brokers.ToList()[counter];

                if (counter == data.Exchanges[exchangeId].Brokers.Count)
                {
                    Interlocked.Exchange(ref putOrdersCounter, 0);
                }

                var result = broker.ProcessMatchingOrders(order, data.Exchanges[exchangeId].Orders.Values.Where(x => x.ShareName.Equals(order.ShareName) && x.Type != order.Type && x.Status != Order.OrderStatus.DONE && x.Status != Order.OrderStatus.DELETED));
                ProcessOrder(order, result, exchangeId);
            }
        }

        private void ProcessOrder(Order order, OrderMatchResult result, string exchangeId)
        {
            if (result.Order != null) // else punish : do nothing
            {
                foreach (Order o in result.Matches)
                {
                    data.Exchanges[exchangeId].Orders[o.Id] = o;
                    NotifySubscribers(data.Exchanges[exchangeId].OrderCallbacks, o);
                }

                foreach (Transaction t in result.Transactions)
                {
                    InvestorDepot buyer = GetInvestorDepot(t.BuyerId, exchangeId);
                    buyer.Budget -= (t.TotalCost + t.BuyerProvision + t.FundProvision);
                    int val;
                    buyer.Shares[order.ShareName] = buyer.Shares.TryGetValue(order.ShareName, out val) ? val + t.NoOfSharesSold : t.NoOfSharesSold;
                    InvestorDepot seller = GetInvestorDepot(t.SellerId, exchangeId);
                    if (seller != null) // seller is investor (which could also be a fund)
                    {
                        seller.Budget += (t.TotalCost - t.SellerProvision - t.FundProvision);
                        seller.Shares[order.ShareName] -= t.NoOfSharesSold;
                    }
                    else // seller is firm
                    {
                        var firm = data.Exchanges[exchangeId].FirmDepots[t.SellerId];
                        firm.OwnedShares -= t.NoOfSharesSold;
                    }
                    if (t.IsFund) // sold share is fund share
                    {
                        var fund = data.Exchanges[exchangeId].FundDepots[t.ShareName];
                        fund.Budget += t.FundProvision * 2;
                        NotifySubscribers(data.Exchanges[exchangeId].FundCallbacks, fund);
                    }

                    data.Exchanges[exchangeId].Transactions.Add(t);
                    NotifySubscribers(data.Exchanges[exchangeId].TransactionCallbacks, t);
                    NotifySubscribers(data.Exchanges[exchangeId].InvestorCallbacks, buyer);
                    NotifySubscribers(data.Exchanges[exchangeId].InvestorCallbacks, seller);
                }
                NotifySubscribers(data.Exchanges[exchangeId].OrderCallbacks, result.Order);
                data.Exchanges[exchangeId].Orders[order.Id] = result.Order;

                var info = data.Exchanges[exchangeId].ShareInformation[order.ShareName];
                info.PurchasingVolume = CalculatePurchasingVolume(data.Exchanges[exchangeId].Orders.Values);
                info.SalesVolume = CalculateSalesVolume(data.Exchanges[exchangeId].Orders.Values);
                NotifySubscribers(data.Exchanges[exchangeId].ShareInformationCallbacks, info);
            }
            else
            {
                data.Exchanges[exchangeId].Orders.TryRemove(order.Id, out order);
            }
        }

        public void DeleteOrder(Order order, string exchangeId)
        {
            Order o;
            data.Exchanges[exchangeId].Orders.TryRemove(order.Id, out o);
        }

        public IEnumerable<Transaction> GetTransactions(string exchangeId)
        {
            return data.Exchanges[exchangeId].Transactions;
        }

        public void PutTransaction(Transaction transaction, string exchangeId)
        {
            data.Exchanges[exchangeId].Transactions.Add(transaction);
            NotifySubscribers(data.Exchanges[exchangeId].TransactionCallbacks, transaction);
        }

        public FirmDepot GetFirmDepot(string firmName, string exchangeId)
        {
            FirmDepot result;
            data.Exchanges[exchangeId].FirmDepots.TryGetValue(firmName, out result);
            return result;
        }

        public FirmDepot RegisterFirm(FirmRegistration request, string exchangeId)
        {
            if (data.Exchanges[exchangeId].Brokers.Count > 0)
            {
                FirmRequestResult result = data.Exchanges[exchangeId].Brokers.First().ProcessFirmRegistration(request) as FirmRequestResult;
                var depot = result.FirmDepot;
                var info = result.ShareInformation;
                var order = result.Order;
                data.Exchanges[exchangeId].FirmDepots[depot.FirmName] = depot;
                data.Exchanges[exchangeId].Orders[order.Id] = order;
                NotifySubscribers(data.Exchanges[exchangeId].OrderCallbacks, order);
                data.Exchanges[exchangeId].ShareInformation[info.FirmName] = info;
                NotifySubscribers(data.Exchanges[exchangeId].ShareInformationCallbacks, info);
                return result.FirmDepot;
            }
            else
            {
                data.Exchanges[exchangeId].PendingFirmRegistrationRequests.Add(request);
                // TODO implement mechanism to call brokers when coming online.
                return null;
            }
        }

        public void SubscribeOnNewRegistrationRequestAvailable(string exchangeId)
        {
            var subscriber = OperationContext.Current.GetCallbackChannel<IWallstreetSubscriber>();
            data.Exchanges[exchangeId].ShareInformationCallbacks.Add(subscriber.OnNewShareInformationAvailable);
        }

        public void SubscribeOnNewShareInformationAvailable(string exchangeId)
        {
            var subscriber = OperationContext.Current.GetCallbackChannel<IWallstreetSubscriber>();
            data.Exchanges[exchangeId].ShareInformationCallbacks.Add(subscriber.OnNewShareInformationAvailable);
        }

        public void SubscribeOnNewFundDepotAvailable(string exchangeId)
        {
            var subscriber = OperationContext.Current.GetCallbackChannel<IWallstreetSubscriber>();
            data.Exchanges[exchangeId].FundCallbacks.Add(subscriber.OnNewFundDepotAvailable);
        }

        public void SubscribeOnNewOrderAvailable(string exchangeId)
        {
            var subscriber = OperationContext.Current.GetCallbackChannel<IWallstreetSubscriber>();
            data.Exchanges[exchangeId].OrderCallbacks.Add(subscriber.OnNewOrderAvailable);
        }

        public void SubscribeOnNewTransactionAvailable(string exchangeId)
        {
            var subscriber = OperationContext.Current.GetCallbackChannel<IWallstreetSubscriber>();
            data.Exchanges[exchangeId].TransactionCallbacks.Add(subscriber.OnNewTransactionAvailable);
        }

        public void SubscribeOnNewInvestorDepotAvailable(string exchangeId)
        {
            var subscriber = OperationContext.Current.GetCallbackChannel<IWallstreetSubscriber>();
            data.Exchanges[exchangeId].InvestorCallbacks.Add(subscriber.OnNewInvestorDepotAvailable);
        }

        public void RegisterBroker(string exchangeId)
        {
            bool empty = data.Exchanges[exchangeId].Brokers.Count == 0;
            var subscriber = OperationContext.Current.GetCallbackChannel<IBroker>();
            data.Exchanges[exchangeId].Brokers.Add(subscriber);

            if (empty && data.Exchanges[exchangeId].Brokers.Count == 1)
            {
                Order[] pending = data.Exchanges[exchangeId].Orders.Values.Where(x => x.Status != Order.OrderStatus.DONE && x.Status != Order.OrderStatus.DELETED).ToArray();

                foreach (Order o in pending)
                {
                    PutOrder(o, exchangeId);
                }
            }

            if (data.Exchanges[exchangeId].PendingFirmRegistrationRequests.Count > 0)
            {
                foreach (FirmRegistration r in data.Exchanges[exchangeId].PendingFirmRegistrationRequests)
                {
                    RegisterFirm(r, exchangeId);
                }
            }

            if (data.Exchanges[exchangeId].PendingFundRegistrationRequests.Count > 0)
            {
                foreach (FundRegistration r in data.Exchanges[exchangeId].PendingFundRegistrationRequests)
                {
                    LoginFund(r, exchangeId);
                }
            }
        }

        public void UnregisterBroker(string exchangeId)
        {
            var subscriber = OperationContext.Current.GetCallbackChannel<IBroker>();
            data.Exchanges[exchangeId].Brokers = new ConcurrentBag<IBroker>(data.Exchanges[exchangeId].Brokers.Where(x => x.GetHashCode() != subscriber.GetHashCode()));
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
