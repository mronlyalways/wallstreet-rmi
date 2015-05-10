using BrokerService.localhost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace BrokerService
{
    [ServiceContract(CallbackContract = typeof(IBroker))]
    public interface IBrokerService
    {
        [OperationContract]
        // Registers a broker
        void Register();

        // Called by WallstreetWebservices to register new Firm
        FirmDepot RegisterFirm(Request request);
    }
}
