using Broker.localhost;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public Tuple<string, int, int, double, int, int, Tuple<string, int>> OnNewRegistrationRequestAvailable(Request request)
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
                info = new ShareInformation() { FirmName = firmName, NoOfShares = request.Shares, PricePerShare = request.PricePerShare, PurchasingVolume = 0, SalesVolume = request.Shares };
            }

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

            return new Tuple<string, int, int, double, int, int, Tuple<string, int>>(firmName, depot.OwnedShares, info.NoOfShares, info.PricePerShare, info.PurchasingVolume, info.SalesVolume, new Tuple<string, int>(order.Id, order.TotalNoOfShares));
        }
    }
}
