using GalaSoft.MvvmLight;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Investor.Model;
using System;
using GalaSoft.MvvmLight.Command;
using Investor.localhost;
using System.Threading.Tasks;

namespace Investor.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private IDataService data;

        public MainViewModel(IDataService data, IEnumerable<string> exchangeNames)
        {
            this.data = data;
            Depots = new ObservableCollection<InvestorDepot>();
            MarketInformation = new ObservableCollection<ShareInformation>();
            OwnedShares = new ObservableCollection<OwningShareDTO>();
            PendingOrders = new ObservableCollection<Order>();
            var locator = (ViewModelLocator) App.Current.Resources["Locator"];
            foreach (string name in locator.RegisteredExchanges)
            {
                Depots.Add(data.LoadInvestorInformation(name));
                MarketInformation = new ObservableCollection<ShareInformation>(MarketInformation.Union(data.LoadMarketInformation(name)));
                PendingOrders = new ObservableCollection<Order>(PendingOrders.Union(data.LoadPendingOrders(name)));
            }
            ActiveDepot = Depots.First();
            data.AddNewInvestorInformationAvailableCallback(UpdateInvestorInformation);
            data.AddNewMarketInformationAvailableCallback(UpdateShareInformation);
            data.AddNewOrderAvailableCallback(UpdateOrderInformation);
            PlaceBuyingOrderCommand = new RelayCommand(PlaceBuyingOrder, () => SelectedBuyingShare != null && ActiveDepot != null);
            PlaceSellingOrderCommand = new RelayCommand(PlaceSellingOrder, () => SelectedSellingShare != null && ActiveDepot != null);
            CancelPendingOrderCommand = new RelayCommand(CancelPendingOrder, () => SelectedPendingOrder != null && SelectedPendingOrder.Status == OrderStatus.OPEN);
            LogoutCommand = new RelayCommand(Logout, () => true); 
            
            UpdateOwnedShares();
        }

        private void UpdateOrderInformation(Order order)
        {
            PendingOrders = new ObservableCollection<Order>(PendingOrders.Where(x => !x.Id.Equals(order.Id) && x.Status != OrderStatus.DONE));
            if (order.Status != OrderStatus.DONE)
            {
                PendingOrders.Add(order);
            }
        }

        private void UpdateInvestorInformation(InvestorDepot d)
        {
            var tmp = new ObservableCollection<InvestorDepot>(Depots.Where(x => x.ExchangeName != d.ExchangeName));
            tmp.Add(d);
            Depots = tmp;
            UpdateOwnedShares();
        }

        private void UpdateShareInformation(ShareInformation info)
        {
            MarketInformation = new ObservableCollection<ShareInformation>(MarketInformation.Where(x => x.FirmName != info.FirmName));
            MarketInformation.Add(info);
            MarketInformation = new ObservableCollection<ShareInformation>(from i in MarketInformation orderby i.FirmName select i);
            UpdateOwnedShares();
        }

        private void UpdateOwnedShares()
        {
            var collection = new ObservableCollection<OwningShareDTO>();

            foreach (InvestorDepot depot in Depots)
            {
                foreach (string shareName in depot.Shares.Keys)
                {
                    var infos = MarketInformation.Where(x => x.FirmName == shareName).ToList();
                    ShareInformation info = infos.FirstOrDefault();
                    if (info != null)
                    {
                        OwningShareDTO s = new OwningShareDTO()
                        {
                            ShareName = shareName,
                            Amount = depot.Shares[shareName],
                            StockPrice = info.PricePerShare,
                            ExchangeName = info.ExchangeName
                        };
                        collection.Add(s);
                    }
                }
            }

            OwnedShares = collection;

            RaisePropertyChanged(() => DepotValue);

        }

        public double DepotValue
        {
            get
            {
                double value = 0;
                foreach (OwningShareDTO s in OwnedShares)
                {
                    value += s.Value;
                }
                return value;
            }
        }

        private InvestorDepot activeDepot;
        public InvestorDepot ActiveDepot
        {
            get
            {
                return activeDepot;
            }
            set
            {
                if (value != null)
                {
                    activeDepot = value;
                }
                else
                {
                    activeDepot = Depots.First();
                }
                RaisePropertyChanged(() => ActiveDepot);
                RaisePropertyChanged(() => ActiveMarketInformation);
            }
        }

        private ObservableCollection<InvestorDepot> depots;
        public ObservableCollection<InvestorDepot> Depots
        {
            get
            {
                return depots;
            }
            set
            {
                depots = value;
                RaisePropertyChanged(() => Depots);
            }
        }

        public ObservableCollection<ShareInformation> ActiveMarketInformation
        {
            get
            {
                return new ObservableCollection<ShareInformation>(MarketInformation.Where(x => x.ExchangeName.Equals(ActiveDepot.ExchangeName)));
            }
        }

        private ObservableCollection<ShareInformation> marketInformation;
        public ObservableCollection<ShareInformation> MarketInformation
        {
            get
            {
                return marketInformation;
            }
            set
            {
                marketInformation = new ObservableCollection<ShareInformation>(from i in value orderby i.FirmName select i);
                RaisePropertyChanged(() => MarketInformation);
                RaisePropertyChanged(() => ActiveMarketInformation);
            }
        }

        private ObservableCollection<OwningShareDTO> ownedShares;
        public ObservableCollection<OwningShareDTO> OwnedShares
        {
            get
            {
                return ownedShares;
            }
            set
            {
                ownedShares = new ObservableCollection<OwningShareDTO>(from i in value orderby i.ShareName select i); ;
                RaisePropertyChanged(() => OwnedShares);
            }
        }

        private ObservableCollection<Order> pendingOrders;
        public ObservableCollection<Order> PendingOrders
        {
            get
            {
                return pendingOrders;
            }
            set
            {
                pendingOrders = new ObservableCollection<Order>(from i in value orderby i.Id select i);
                RaisePropertyChanged(() => PendingOrders);
            }
        }

        private ShareInformation selectedBuyingShare;
        public ShareInformation SelectedBuyingShare
        {
            get
            {
                return selectedBuyingShare;
            }
            set
            {
                selectedBuyingShare = value;
                RaisePropertyChanged(() => SelectedBuyingShare);
                PlaceBuyingOrderCommand.RaiseCanExecuteChanged();
            }
        }

        private OwningShareDTO selectedSellingShare;
        public OwningShareDTO SelectedSellingShare
        {
            get
            {
                return selectedSellingShare;
            }
            set
            {
                selectedSellingShare = value;
                RaisePropertyChanged(() => SelectedSellingShare);
                PlaceSellingOrderCommand.RaiseCanExecuteChanged();
            }
        }

        private Order selectedPendingOrder;
        public Order SelectedPendingOrder
        {
            get
            {
                return selectedPendingOrder;
            }
            set
            {
                selectedPendingOrder = value;
                RaisePropertyChanged(() => SelectedPendingOrder);
                CancelPendingOrderCommand.RaiseCanExecuteChanged();
            }
        }

        private int noOfSharesBuying;
        public int NoOfSharesBuying
        {
            get
            {
                return noOfSharesBuying;
            }
            set
            {
                noOfSharesBuying = value;
                RaisePropertyChanged(() => NoOfSharesBuying);
            }
        }

        private int noOfSharesSelling;
        public int NoOfSharesSelling
        {
            get
            {
                return noOfSharesSelling;
            }
            set
            {
                noOfSharesSelling = value;
                RaisePropertyChanged(() => NoOfSharesSelling);
            }
        }

        private double upperPriceLimit;
        public double UpperPriceLimit
        {
            get
            {
                return upperPriceLimit;
            }
            set
            {
                upperPriceLimit = value;
                RaisePropertyChanged(() => UpperPriceLimit);
            }
        }

        private double lowerPriceLimit;
        public double LowerPriceLimit
        {
            get
            {
                return lowerPriceLimit;
            }
            set
            {
                lowerPriceLimit = value;
                RaisePropertyChanged(() => LowerPriceLimit);
            }
        }

        private bool prioritizeBuying;
        public bool PrioritizeBuying
        {
            get
            {
                return prioritizeBuying;
            }
            set
            {
                prioritizeBuying = value;
                RaisePropertyChanged(() => PrioritizeBuying);
            }
        }

        private bool prioritizeSelling;
        public bool PrioritizeSelling
        {
            get
            {
                return prioritizeSelling;
            }
            set
            {
                prioritizeSelling = value;
                RaisePropertyChanged(() => PrioritizeSelling);
            }
        }

        public RelayCommand PlaceBuyingOrderCommand { get; private set; }

        public RelayCommand PlaceSellingOrderCommand { get; private set; }

        public RelayCommand CancelPendingOrderCommand { get; private set; }

        public RelayCommand LogoutCommand { get; private set; }

        private void PlaceBuyingOrder()
        {
            var id = ActiveDepot.Id + DateTime.Now.Ticks.ToString();
            var order = new Order() { Id = id, InvestorId = ActiveDepot.Id, Type = OrderType.BUY, ShareName = SelectedBuyingShare.FirmName, Limit = UpperPriceLimit, TotalNoOfShares = NoOfSharesBuying, NoOfProcessedShares = 0, Prioritize = PrioritizeBuying };
            data.PlaceOrder(order, ActiveDepot.ExchangeName);
        }

        private void PlaceSellingOrder()
        {
            var id = ActiveDepot.Id + DateTime.Now.Ticks.ToString();
            var order = new Order() { Id = id, InvestorId = ActiveDepot.Id, Type = OrderType.SELL, ShareName = SelectedSellingShare.ShareName, Limit = LowerPriceLimit, TotalNoOfShares = NoOfSharesSelling, NoOfProcessedShares = 0, Prioritize = PrioritizeSelling };
            data.PlaceOrder(order, SelectedSellingShare.ExchangeName);
        }

        private void CancelPendingOrder()
        {
            data.CancelOrder(SelectedPendingOrder, ActiveDepot.ExchangeName);
            PendingOrders.Remove(SelectedPendingOrder);
        }

        private void Logout()
        {
            data.Dispose();
            App.Current.Shutdown();
        }
    }
}