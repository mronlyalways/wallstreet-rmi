using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using WallstreetDataService.Model;

namespace WallstreetDataService
{
    [ServiceContract(CallbackContract = typeof(IWallstreetSubscriber))]
    public interface IWallstreetDataService
    {
        [OperationContract]
        IEnumerable<ShareInformation> GetMarketInformation();

        [OperationContract]
        ShareInformation GetShareInformation(string shareName);

        [OperationContract(IsOneWay = true)]
        void PutShareInformation(ShareInformation info);

        [OperationContract]
        IEnumerable<InvestorDepot> GetInvestorInformation();

        [OperationContract]
        InvestorDepot GetInvestorDepot(string investorId);

        [OperationContract(IsOneWay = true)]
        void PutInvestorDepot(InvestorDepot investor);

        [OperationContract]
        InvestorDepot LoginInvestor(Registration registration);

        [OperationContract]
        IEnumerable<Order> GetOrders();

        [OperationContract]
        IEnumerable<Order> GetPendingOrders(string investorId);

        [OperationContract(IsOneWay = true)]
        void PutOrder(Order order);

        [OperationContract(IsOneWay = true)]
        void DeleteOrder(Order order);

        [OperationContract]
        IEnumerable<Transaction> GetTransactions();

        [OperationContract(IsOneWay = true)]
        void PutTransaction(Transaction transaction);

        [OperationContract]
        FirmDepot RegisterFirm(Request request);

        [OperationContract]
        FirmDepot GetFirmDepot(string firmName);

        [OperationContract(IsOneWay = true)]
        void SubscribeOnNewShareInformationAvailable();

        [OperationContract(IsOneWay = true)]
        void SubscribeOnNewOrderAvailable();

        [OperationContract(IsOneWay = true)]
        void SubscribeOnNewInvestorDepotAvailable();

        [OperationContract(IsOneWay = true)]
        void SubscribeOnNewTransactionAvailable();
    }
}
