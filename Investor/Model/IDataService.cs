using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using Investor.localhost;

namespace Investor.Model
{
    public interface IDataService : IDisposable
    {
        InvestorDepot Login(InvestorRegistration r, string exchangeId);

        void PlaceOrder(Order order, string exchangeId);

        void CancelOrder(Order order, string exchangeId);

        InvestorDepot LoadInvestorInformation(string exchangeId);

        IEnumerable<string> LoadExchangeInformation();

        IEnumerable<ShareInformation> LoadMarketInformation(string exchangeId);

        IEnumerable<Order> LoadPendingOrders(string exchangeId);
        
        void AddNewMarketInformationAvailableCallback(Action<ShareInformation> callback);

        void AddNewInvestorInformationAvailableCallback(Action<InvestorDepot> callback);

        void AddNewOrderAvailableCallback(Action<Order> callback);

        void OnNewShareInformationAvailable(ShareInformation info);

        void OnNewOrderAvailable(Order order);

        void OnNewInvestorDepotAvailable(InvestorDepot depot);

        void OnNewTransactionAvailable(Transaction transaction);
    }
}
