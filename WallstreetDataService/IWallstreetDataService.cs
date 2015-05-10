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

        [OperationContract]
        void PutShareInformation(ShareInformation info);

        [OperationContract]
        IEnumerable<Order> GetOrders();

        [OperationContract]
        void PutOrder(Order order);

        [OperationContract]
        IEnumerable<Transaction> GetTransactions();

        [OperationContract]
        void PutTransaction(Transaction transaction);

        [OperationContract]
        FirmDepot RegisterFirm(Request request);

        [OperationContract]
        FirmDepot GetFirmDepot(string firmName);

        [OperationContract]
        void SubscribeOnNewShareInformationAvailable();

        [OperationContract]
        void SubscribeOnNewOrderAvailable();

        [OperationContract]
        void SubscribeOnNewTransactionAvailable();
    }
}
