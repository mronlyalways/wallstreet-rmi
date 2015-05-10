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
        Tuple<string, int, int, double, int, int, Tuple<string, int>> OnNewRegistrationRequestAvailable(Request info);
    }
}
