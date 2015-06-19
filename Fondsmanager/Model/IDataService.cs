using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using FundManager.localhost;

namespace FundManager.Model
{
    public interface IDataService : IDisposable
    {
        void Login(FundRegistration r);

        void PlaceOrder(Order order);

        void CancelOrder(Order order);

        FundDepot LoadFundInformation();

        IEnumerable<ShareInformation> LoadMarketInformation();

        IEnumerable<Order> LoadPendingOrders();

        void AddNewMarketInformationAvailableCallback(Action<ShareInformation> callback);

        void AddNewFundInformationAvailableCallback(Action<FundDepot> callback);

        void RemoveNewFundInformationAvailableCallback(Action<FundDepot> callback);

        void AddNewInvestorInformationAvailableCallback(Action<InvestorDepot> callback);

        void AddNewOrderAvailableCallback(Action<Order> callback);

        void OnNewShareInformationAvailable(ShareInformation info);

        void OnNewOrderAvailable(Order order);

        void OnNewFundDepotAvailable(FundDepot depot);

        void OnNewTransactionAvailable(Transaction transaction);
    }
}
