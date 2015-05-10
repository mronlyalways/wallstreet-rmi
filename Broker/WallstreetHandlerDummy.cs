using Broker.localhost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Broker
{
    public class WallstreetHandlerDummy : IWallstreetDataServiceCallback
    {
        public void OnNewShareInformationAvailable(ShareInformation info) { }

        public void OnNewOrderAvailable(Order order) { }

        public void OnNewTransactionAvailable(Transaction transaction) { }
    }
}
