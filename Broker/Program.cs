using Broker.localhost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using XcoSpaces;
using XcoSpaces.Collections;
using XcoSpaces.Exceptions;

namespace Broker
{
    public class Program
    {
        static void Main(string[] args)
        {
            var wallstreetClient = new WallstreetDataServiceClient(new InstanceContext(new WallstreetHandlerDummy()));
            var handler = new BrokerHandler(wallstreetClient);
            BrokerServiceClient client = new BrokerServiceClient(new InstanceContext(handler));
            client.RegisterBroker();
            Console.WriteLine("Broker online. Press enter to exit ...");
            Console.ReadLine();
        }

        private static void HandleRequests()
        {
                //try
                //{
                //    requestsQ = space.Get<XcoQueue<Request>>("RequestQ", spaceServer);
                //    Request request;
                //    FirmDepot depot;
                //    while (true)
                //    {
                //        request = requestsQ.Dequeue(-1);
                //        using (XcoTransaction transaction = space.BeginTransaction())
                //        {
                //            try
                //            {
                //                if (firmDepots.TryGetValue(request.FirmName, out depot))
                //                {
                //                    depot.OwnedShares += request.Shares;
                //                    firmDepots[request.FirmName] = depot;
                //                    var info = stockInformation[request.FirmName];
                //                    stockInformation[request.FirmName] = new Tuple<int, double>(info.Item1 + request.Shares, info.Item2);
                //                    Console.WriteLine("Add {0} shares to existing account \"{1}\"", request.Shares, request.FirmName);
                //                }
                //                else
                //                {
                //                    firmDepots.Add(request.FirmName, new FirmDepot() { FirmName = request.FirmName, OwnedShares = request.Shares });
                //                    stockInformation.Add(request.FirmName, new Tuple<int, double>(request.Shares, request.PricePerShare));
                //                    Console.WriteLine("Create new firm depot for \"{0}\" with {1} shares, selling for {2}", request.FirmName, request.Shares, request.PricePerShare);
                //                }
                //                var orderId = request.FirmName + DateTime.Now.Ticks.ToString();
                //                Order o = new Order()
                //                {
                //                    Id = orderId,
                //                    InvestorId = request.FirmName,
                //                    ShareName = request.FirmName,
                //                    Type = Order.OrderType.SELL,
                //                    Limit = 0,
                //                    NoOfProcessedShares = 0,
                //                    TotalNoOfShares = request.Shares
                //                };
                //                orderQueue.Enqueue(o);
                //                transaction.Commit();
                //            }
                //            catch (Exception)
                //            {
                //                transaction.Rollback();
                //                requestsQ.Enqueue(request);
                //            }
                //        }
                //    }
                //}
                //catch (XcoException e)
                //{
                //    Console.WriteLine(e.StackTrace);
                //    Console.WriteLine("Unable to reach server.\nPress enter to exit.");
                //    Console.ReadLine();
                //}
        }
    }
}
