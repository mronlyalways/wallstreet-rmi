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
        IEnumerable<string> GetExchanges();

        [OperationContract]
        IEnumerable<ShareInformation> GetMarketInformation(string exchangeId);

        [OperationContract]
        IEnumerable<ShareInformation> GetOverallMarketInformation();

        [OperationContract]
        ShareInformation GetShareInformation(string shareName, string exchangeId);

        [OperationContract(IsOneWay = true)]
        void PutShareInformation(ShareInformation info, string exchangeId);

        [OperationContract]
        IEnumerable<InvestorDepot> GetInvestorInformation(string exchangeId);

        [OperationContract]
        InvestorDepot GetInvestorDepot(string investorId, string exchangeId);

        [OperationContract(IsOneWay = true)]
        void PutInvestorDepot(InvestorDepot investor, string exchangeId);

        [OperationContract]
        InvestorDepot LoginInvestor(InvestorRegistration registration, string exchangeId);

        [OperationContract]
        IEnumerable<string> GetRegisteredExchanges(string investorId);

        [OperationContract]
        FundDepot GetFundDepot(string fundId, string exchangeId);

        [OperationContract]
        FundDepot GetOverallFundInformation(string fundId);

        [OperationContract(IsOneWay = true)]
        void LoginFund(FundRegistration registration, string exchangeId);

        [OperationContract]
        IEnumerable<Order> GetOrders(string exchangeId);

        [OperationContract]
        IEnumerable<Order> GetOverallOrders();

        [OperationContract]
        IEnumerable<Order> GetPendingOrders(string investorId, string exchangeId);

        [OperationContract(IsOneWay = true)]
        void PutOrder(Order order, string exchangeId);

        [OperationContract(IsOneWay = true)]
        void DeleteOrder(Order order, string exchangeId);

        [OperationContract]
        IEnumerable<Transaction> GetTransactions(string exchangeId);

        [OperationContract(IsOneWay = true)]
        void PutTransaction(Transaction transaction, string exchangeId);

        [OperationContract]
        FirmDepot RegisterFirm(FirmRegistration request, string exchangeId);

        [OperationContract]
        FirmDepot GetFirmDepot(string firmName, string exchangeId);

        [OperationContract(IsOneWay = true)]
        void SubscribeOnNewShareInformationAvailable(string exchangeId);

        [OperationContract(IsOneWay = true)]
        void SubscribeOnNewFundDepotAvailable(string exchangeId);

        [OperationContract(IsOneWay = true)]
        void SubscribeOnNewOrderAvailable(string exchangeId);

        [OperationContract(IsOneWay = true)]
        void SubscribeOnNewInvestorDepotAvailable(string exchangeId);

        [OperationContract(IsOneWay = true)]
        void SubscribeOnNewTransactionAvailable(string exchangeId);
    }
}
