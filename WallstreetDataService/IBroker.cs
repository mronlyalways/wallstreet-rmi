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
        FirmRequestResult OnNewRegistrationRequestAvailable(Request request);

        [OperationContract]
        OrderMatchResult OnNewOrderMatchingRequestAvailable(Order order, IEnumerable<Order> orders);
    }
}
