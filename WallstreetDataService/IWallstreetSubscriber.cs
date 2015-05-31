using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using WallstreetDataService.Model;

namespace WallstreetDataService
{
    public interface IWallstreetSubscriber
    {
        [OperationContract(IsOneWay = true)]
        void OnNewInvestorDepotAvailable(InvestorDepot depot);

        [OperationContract(IsOneWay = true)]
        void OnNewFundDepotAvailable(FundDepot depot);

        [OperationContract(IsOneWay = true)]
        void OnNewShareInformationAvailable(ShareInformation info);

        [OperationContract(IsOneWay = true)]
        void OnNewOrderAvailable(Order order);

        [OperationContract(IsOneWay = true)]
        void OnNewTransactionAvailable(Transaction transaction);
    }
}
