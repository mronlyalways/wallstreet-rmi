using Firm.localhost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Firm
{
    class Program
    {
        static void Main(string[] args)
        {
            var wallstreetClient = new WallstreetDataServiceClient(new InstanceContext(new WallstreetHandlerDummy()));

            string name, exchange;
            int shares;
            double pricePerShare;
            if (args.Count() == 4)
            {
                exchange = args[0];
                name = args[1];
                shares = Int32.Parse(args[2]);
                pricePerShare = Double.Parse(args[3]);
            }
            else
            {
                Console.WriteLine("Enter a quadruple of <ExchangeName> <FirmName> <NoOfShares> <PricePerShare> to create a firm.");
                var input = Console.ReadLine().Split(' ');
                exchange = input[0];
                name = input[1];
                shares = Int32.Parse(input[2]);
                pricePerShare = Double.Parse(input[3]);
            }
            var depot = wallstreetClient.RegisterFirm(new FirmRegistration { Id = name, Shares = shares, PricePerShare = pricePerShare }, exchange);
            Console.WriteLine("Depot created.");

            Thread.Sleep(2000);
        }
    }
}
