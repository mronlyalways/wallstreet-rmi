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
        void Login(FundRegistration r, string exchangeId);

        void PlaceOrder(Order order, string exchangeId);

        void CancelOrder(Order order, string exchangeId);

        FundDepot LoadFundInformation(string exchangeId);

        IEnumerable<string> LoadExchangeInformation();

        IEnumerable<ShareInformation> LoadMarketInformation(string exchangeId);

        IEnumerable<Order> LoadPendingOrders(string exchangeId);

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
