using BrokerService.localhost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace BrokerService
{
    public interface IBroker
    {
        [OperationContract]
        Tuple<FirmDepot, ShareInformation, Order> OnNewRegistrationRequestAvailable(Request info);
    }
}
