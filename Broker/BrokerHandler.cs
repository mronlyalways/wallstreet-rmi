using Broker.localhost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Broker
{
    public class BrokerHandler : IBrokerServiceCallback
    {
        private WallstreetDataServiceClient wallstreetClient;
        private string exchangeId;

        public BrokerHandler(WallstreetDataServiceClient wallstreetClient, string exchangeId)
        {
            this.wallstreetClient = wallstreetClient;
            this.exchangeId = exchangeId;
        }

        public FirmRequestResult ProcessFirmRegistration(FirmRegistration request)
        {
            var firmName = request.Id;
            var depot = wallstreetClient.GetFirmDepot(firmName, exchangeId);

            if (depot == null)
            {
                depot = new FirmDepot() { FirmName = firmName, OwnedShares = 0 };
            }

            depot.OwnedShares += request.Shares;

            var info = wallstreetClient.GetShareInformation(firmName, exchangeId);
            if (info == null)
            {
                info = new ShareInformation { FirmName = firmName, NoOfShares = 0, PricePerShare = request.PricePerShare, PurchasingVolume = 0, SalesVolume = 0, ExchangeName = exchangeId };
            }
            info.NoOfShares += request.Shares;
            info.SalesVolume += request.Shares;
            var order = new Order()
            {
                Id = firmName + DateTime.Now.Ticks,
                ShareName = firmName,
                InvestorId = firmName,
                Type = OrderType.SELL,
                TotalNoOfShares = request.Shares,
                NoOfOpenShares = request.Shares,
                NoOfProcessedShares = 0,
                Status = OrderStatus.OPEN,
                Limit = 0,
                Prioritize = false,
                IsFundShare = false
            };

            return new FirmRequestResult { FirmDepot = depot, ShareInformation = info, Order = order };
        }

        public FundRequestResult ProcessFundRegistration(FundRegistration request)
        {
            var fundName = request.Id;
            var depot = new FundDepot
            {
                Id = fundName,
                ExchangeName = exchangeId,
                Budget = request.FundAssets,
                Shares = new Dictionary<string, int>()
            };
            ShareInformation info = null;
            Order order = null;
            if (request.Shares > 0)
            {
                var exchanges = wallstreetClient.GetExchanges();
                var list = new List<FundDepot>();
                foreach (string e in exchanges)
                {
                    var fund = wallstreetClient.GetFundDepot(fundName, e);
                    if (fund != null)
                    {
                        list.Add(fund);
                    }
                }
                var assets = list.Sum(x => x.Budget);
                assets += request.FundAssets;

                depot.Shares.Add(fundName, request.Shares);
                info = new ShareInformation
                {
                    FirmName = fundName,
                    NoOfShares = request.Shares,
                    PricePerShare = assets / request.Shares,
                    PurchasingVolume = 0,
                    SalesVolume = request.Shares,
                    IsFund = true,
                    ExchangeName = exchangeId
                };
                order = new Order
                {
                    Id = fundName + DateTime.Now.Ticks,
                    ShareName = fundName,
                    InvestorId = fundName,
                    Type = OrderType.SELL,
                    TotalNoOfShares = request.Shares,
                    NoOfOpenShares = request.Shares,
                    NoOfProcessedShares = 0,
                    Status = OrderStatus.OPEN,
                    Limit = 0,
                    Prioritize = false,
                    IsFundShare = true
                };
            }
            return new FundRequestResult { FundDepot = depot, ShareInformation = info, Order = order };
        }

        public OrderMatchResult ProcessMatchingOrders(Order order, Order[] orders)
        {
            Console.WriteLine("Process order: " + order);
            var stockPrice = wallstreetClient.GetShareInformation(order.ShareName, exchangeId).PricePerShare;
            var result = MatchOrders(order, orders.ToList(), stockPrice);
            var newOrder = result.Item1;
            var matches = result.Item2;
            var newTransactions = result.Item3;
            if ((order.Type == OrderType.BUY && IsAffordableForBuyer(order.InvestorId, newTransactions))
                || (order.Type == OrderType.SELL && SellerHasEnoughShares(order.InvestorId, order.ShareName, newTransactions)))
            {
                return new OrderMatchResult { Order = newOrder, Matches = matches.ToArray(), Transactions = newTransactions.ToArray() };
            }
            else
            {
                return new OrderMatchResult { Order = null, Matches = null, Transactions = null };
            }
        }

        private bool IsAffordableForBuyer(string id, IEnumerable<Transaction> transactions)
        {
            var moneyNeeded = transactions.Where(x => x.BuyerId == id).Sum(x => x.TotalCost + x.BuyerProvision);
            var depot = wallstreetClient.GetInvestorDepot(id, exchangeId);
            return depot.Budget >= moneyNeeded;
        }

        private bool SellerHasEnoughShares(string id, string shareName, IEnumerable<Transaction> transactions)
        {
            var sharesNeeded = transactions.Where(x => x.SellerId == id).Sum(x => x.NoOfSharesSold);
            var seller = wallstreetClient.GetInvestorDepot(id, exchangeId);
            var balance = seller == null ? wallstreetClient.GetFirmDepot(id, exchangeId).OwnedShares : seller.Shares[shareName];
            return balance >= sharesNeeded;
        }

        /// <summary>
        /// Function returns a set of matching orders for the given order and the given counterparts. If you provide a buying order and a set of selling orders,
        /// the result is a tuple consisting of the updated (i.e. adapted processed shares and status) buying order, the set of used selling orders (also updated) and the
        /// transactions that transfer the shares from buyer to seller.
        /// </summary>
        /// <param name="order">Buying or selling order that should be matched</param>
        /// <param name="counterParts">A list of selling orders (in case that order is a buying order) or vice versa</param>
        /// <param name="stockPrice">price that is used for the deal</param>
        /// <returns>A tuple consisting of the updated order, a list of matching (and used) counterpart orders and the respective transactions</returns>
        public Tuple<Order, IEnumerable<Order>, IEnumerable<Transaction>> MatchOrders(Order order, IList<Order> counterParts, double stockPrice)
        {
            var transactions = new List<Transaction>();
            var usedOrders = new List<Order>();
            var sharesProcessedTotal = 0;
            IList<Order> matches;
            bool buyMode = order.Type == OrderType.BUY;

            counterParts = counterParts.OrderBy(x => !x.Prioritize).ToList(); // weirdly, when sorting for Priority, false comes before true

            if (buyMode)
            {
                matches = counterParts.Where(x => x.ShareName.Equals(order.ShareName) && x.Limit <= stockPrice && order.Limit >= stockPrice).ToList();
            }
            else
            {
                matches = counterParts.Where(x => x.ShareName.Equals(order.ShareName) && x.Limit >= stockPrice && order.Limit <= stockPrice).ToList();
            }

            while (matches.Count > 0 && sharesProcessedTotal < order.NoOfOpenShares)
            {
                var match = matches.First();
                var sharesProcessed = Math.Min(order.NoOfOpenShares, match.NoOfOpenShares);
                var totalCost = sharesProcessed * stockPrice;
                var buyerPaysDouble = match.Prioritize && match.Type == OrderType.BUY;
                var sellerPaysDouble = match.Prioritize && match.Type == OrderType.SELL;
                var isFundShare = wallstreetClient.GetFundDepot(match.ShareName, exchangeId) != null;

                transactions.Add(new Transaction()
                {
                    TransactionId = order.Id + match.Id,
                    BrokerId = 1L,
                    ShareName = order.ShareName,
                    BuyerId = buyMode ? order.InvestorId : match.InvestorId,
                    SellerId = buyMode ? match.InvestorId : order.InvestorId,
                    BuyingOrderId = buyMode ? order.Id : match.Id,
                    SellingOrderId = buyMode ? match.Id : order.Id,
                    NoOfSharesSold = sharesProcessed,
                    TotalCost = totalCost,
                    BuyerProvision = totalCost * (buyerPaysDouble ? 0.06 : 0.03),
                    SellerProvision = totalCost * (sellerPaysDouble ? 0.06 : 0.03),
                    FundProvision = isFundShare ? totalCost * 0.02 : 0.0,
                    PricePerShare = stockPrice,
                    IsFund = isFundShare
                });
                order.NoOfProcessedShares += sharesProcessed;
                match.NoOfProcessedShares += sharesProcessed;
                order.Status = (order.NoOfProcessedShares == order.TotalNoOfShares) ? OrderStatus.DONE : OrderStatus.PARTIAL;
                match.Status = (match.NoOfProcessedShares == match.TotalNoOfShares) ? OrderStatus.DONE : OrderStatus.PARTIAL;
                matches.Remove(match);
                usedOrders.Add(match);
                sharesProcessedTotal += sharesProcessed;
            }
            return new Tuple<Order, IEnumerable<Order>, IEnumerable<Transaction>>(order, usedOrders, transactions);
        }
    }
}
