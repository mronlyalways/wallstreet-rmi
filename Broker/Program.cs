using Broker.localhost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Broker
{
    public class Program
    {
        static void Main(string[] args)
        {
            var wallstreetClient = new WallstreetDataServiceClient(new InstanceContext(new WallstreetHandlerDummy()));
            Console.WriteLine("Type in the name of the exchange you want to connect to. Available:");
            var exchanges = wallstreetClient.GetExchanges();
            foreach (string e in exchanges) {
                Console.WriteLine(e);
            }
            var exchangeId = Console.ReadLine();
            var handler = new BrokerHandler(wallstreetClient, exchangeId);
            BrokerServiceClient client = new BrokerServiceClient(new InstanceContext(handler));
            client.RegisterBroker(exchangeId);
            Console.WriteLine("Broker online. Press enter to exit ...");
            Console.ReadLine();
            client.UnregisterBroker(exchangeId);
            client.Close();
        }
    }
}
