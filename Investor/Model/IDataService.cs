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
        InvestorDepot Login(Registration r);

        void PlaceOrder(Order order);

        void CancelOrder(Order order);

        InvestorDepot LoadInvestorInformation();

        IEnumerable<ShareInformation> LoadMarketInformation();

        IEnumerable<Order> LoadPendingOrders();
        
        void AddNewMarketInformationAvailableCallback(Action<ShareInformation> callback);

        void AddNewInvestorInformationAvailableCallback(Action<InvestorDepot> callback);

        void AddNewOrderAvailableCallback(Action<Order> callback);

        void OnNewShareInformationAvailable(ShareInformation info);

        void OnNewOrderAvailable(Order order);

        void OnNewInvestorDepotAvailable(InvestorDepot depot);

        void OnNewTransactionAvailable(Transaction transaction);
    }
}
