using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using WallstreetDataService.Model;

namespace WallstreetDataService
{
    [ServiceContract(CallbackContract = typeof(IBroker))]
    public interface IBrokerService
    {
        [OperationContract]
        void RegisterBroker();

        [OperationContract]
        void UnregisterBroker();
    }
}
