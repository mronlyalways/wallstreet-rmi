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
            var handler = new BrokerHandler(wallstreetClient);
            BrokerServiceClient client = new BrokerServiceClient(new InstanceContext(handler));
            client.RegisterBroker();
            Console.WriteLine("Broker online. Press enter to exit ...");
            Console.ReadLine();
            client.UnregisterBroker();
            client.Close();
        }
    }
}
