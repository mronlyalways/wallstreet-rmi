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
        WallstreetDataServiceClient wallstreetClient;

        public BrokerHandler(WallstreetDataServiceClient wallstreetClient)
        {
            this.wallstreetClient = wallstreetClient;
        }

        public FirmRequestResult OnNewRegistrationRequestAvailable(Request request)
        {
            var firmName = request.FirmName;
            var depot = wallstreetClient.GetFirmDepot(firmName);

            if (depot == null)
            {
                depot = new FirmDepot() { FirmName = firmName, OwnedShares = 0 };
            }

            depot.OwnedShares += request.Shares;

            var info = wallstreetClient.GetShareInformation(firmName);
            if (info == null)
            {
                info = new ShareInformation() { FirmName = firmName, NoOfShares = 0, PricePerShare = request.PricePerShare, PurchasingVolume = 0, SalesVolume = 0 };
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
                Limit = 0
            };

            return new FirmRequestResult { FirmDepot = depot, ShareInformation = info, Order = order };
        }

        public OrderMatchResult OnNewOrderMatchingRequestAvailable(Order order, Order[] orders)
        {
            Console.WriteLine("Process order: " + order);
            var stockPrice = wallstreetClient.GetShareInformation(order.ShareName).PricePerShare;
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
            var moneyNeeded = transactions.Where(x => x.BuyerId == id).Sum(x => x.TotalCost + x.Provision);
            var balance = wallstreetClient.GetInvestorDepot(id).Budget;
            return balance >= moneyNeeded;
        }

        private bool SellerHasEnoughShares(string id, string shareName, IEnumerable<Transaction> transactions)
        {
            var sharesNeeded = transactions.Where(x => x.SellerId == id).Sum(x => x.NoOfSharesSold);
            var seller = wallstreetClient.GetInvestorDepot(id);
            var balance = seller == null ? wallstreetClient.GetFirmDepot(id).OwnedShares : seller.Shares[shareName];
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

            if (buyMode)
            {
                matches = counterParts.Where(x => x.ShareName.Equals(order.ShareName) && x.Limit <= stockPrice && order.Limit >= stockPrice).ToList();
            }
            else
            {
                matches = counterParts.Where(x => x.ShareName.Equals(order.ShareName) && x.Limit >= stockPrice && order.Limit <= stockPrice).ToList();
            }

            while (matches.Count > 0 && sharesProcessedTotal <= order.NoOfOpenShares)
            {
                var match = matches.First();

                var sharesProcessed = Math.Min(order.NoOfOpenShares, match.NoOfOpenShares);
                var totalCost = sharesProcessed * stockPrice;
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
                    Provision = totalCost * 0.03,
                    PricePerShare = stockPrice
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
