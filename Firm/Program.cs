using Firm.localhost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using XcoSpaces;
using XcoSpaces.Collections;
using XcoSpaces.Exceptions;

namespace Firm
{
    class Program
    {
        static void Main(string[] args)
        {
            var wallstreetClient = new WallstreetDataServiceClient(new InstanceContext(new WallstreetHandlerDummy()));

            string name;
            int shares;
            double pricePerShare;
            if (args.Count() == 3)
            {
                name = args[0];
                shares = Int32.Parse(args[1]);
                pricePerShare = Double.Parse(args[2]);
            }
            else
            {
                Console.WriteLine("Enter a triple <FirmName> <NoOfShares> <PricePerShare> to create firm.");
                var input = Console.ReadLine().Split(' ');
                name = input[0];
                shares = Int32.Parse(input[1]);
                pricePerShare = Double.Parse(input[2]);
            }
            var depot = wallstreetClient.RegisterFirm(new Request() { FirmName = name, Shares = shares, PricePerShare = pricePerShare });
            Console.WriteLine("Depot created.");
        }
    }
}
