using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using WallstreetDataService.Model;

namespace WallstreetDataService
{
    public interface IBroker
    {
        [OperationContract]
        FirmRequestResult ProcessFirmRegistration(FirmRegistration request);

        [OperationContract]
        FundRequestResult ProcessFundRegistration(FundRegistration request);

        [OperationContract]
        OrderMatchResult ProcessMatchingOrders(Order order, IEnumerable<Order> orders);
    }
}
