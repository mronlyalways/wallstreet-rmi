using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using Fondsmanager.localhost;

namespace Fondsmanager.Model
{
    public interface IDataService : IDisposable
    {
        FundDepot Login(FundRegistration r);

        void PlaceOrder(Order order);

        void CancelOrder(Order order);

        FundDepot LoadFundInformation();

        IEnumerable<ShareInformation> LoadMarketInformation();

        IEnumerable<Order> LoadPendingOrders();

        void AddNewMarketInformationAvailableCallback(Action<ShareInformation> callback);

        void AddNewFundInformationAvailableCallback(Action<FundDepot> callback);

        void RemoveNewFundInformationAvailableCallback(Action<FundDepot> callback);

        void AddNewOrderAvailableCallback(Action<Order> callback);

        void OnNewShareInformationAvailable(ShareInformation info);

        void OnNewOrderAvailable(Order order);

        void OnNewFundDepotAvailable(FundDepot depot);

        void OnNewTransactionAvailable(Transaction transaction);
    }
}
