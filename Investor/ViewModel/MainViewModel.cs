﻿using GalaSoft.MvvmLight;
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
        private InvestorDepot depot;

        public MainViewModel(IDataService data)
        {
            this.data = data;
            depot = data.LoadInvestorInformation();
            MarketInformation = new ObservableCollection<ShareInformation>(data.LoadMarketInformation());
            OwnedShares = new ObservableCollection<OwningShareDTO>();
            PendingOrders = new ObservableCollection<Order>(data.LoadPendingOrders());
            data.AddNewInvestorInformationAvailableCallback(UpdateInvestorInformation);
            data.AddNewMarketInformationAvailableCallback(UpdateShareInformation);
            data.AddNewOrderAvailableCallback(UpdateOrderInformation);
            PlaceBuyingOrderCommand = new RelayCommand(PlaceBuyingOrder, () => SelectedBuyingShare != null);
            PlaceSellingOrderCommand = new RelayCommand(PlaceSellingOrder, () => SelectedSellingShare != null);
            CancelPendingOrderCommand = new RelayCommand(CancelPendingOrder, () => SelectedPendingOrder != null && SelectedPendingOrder.Status == OrderStatus.OPEN);
            LogoutCommand = new RelayCommand(Logout, () => true); 
            
            UpdateOwnedShares();
        }

        private void UpdateOrderInformation(Order order)
        {
            PendingOrders = new ObservableCollection<Order>(PendingOrders.Where(x => !x.Id.Equals(order.Id) && x.Status != OrderStatus.DONE));
            PendingOrders.Add(order);
        }

        private void UpdateInvestorInformation(InvestorDepot d)
        {
            depot = d;
            RaisePropertyChanged(() => Budget);
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

            foreach (String shareName in depot.Shares.Keys)
            {
                var infos = MarketInformation.Where(x => x.FirmName == shareName).ToList();
                ShareInformation info = infos.First();
                if (info != null)
                {
                    OwningShareDTO s = new OwningShareDTO()
                    {
                        ShareName = shareName,
                        Amount = depot.Shares[shareName],
                        StockPrice = info.PricePerShare
                    };
                    collection.Add(s);
                }
            }

            OwnedShares = collection;

            RaisePropertyChanged(() => DepotValue);

        }

        public string Email { get { return depot.Email; } }

        public double Budget { get { return depot.Budget; } }

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

        public RelayCommand PlaceBuyingOrderCommand { get; private set; }

        public RelayCommand PlaceSellingOrderCommand { get; private set; }

        public RelayCommand CancelPendingOrderCommand { get; private set; }

        public RelayCommand LogoutCommand { get; private set; }

        private void PlaceBuyingOrder()
        {
            var id = Email + DateTime.Now.Ticks.ToString();
            var order = new Order() { Id = id, InvestorId = Email, Type = OrderType.BUY, ShareName = SelectedBuyingShare.FirmName, Limit = UpperPriceLimit, TotalNoOfShares = NoOfSharesBuying, NoOfProcessedShares = 0 };
            data.PlaceOrder(order);
        }

        private void PlaceSellingOrder()
        {
            var id = Email + DateTime.Now.Ticks.ToString();
            var order = new Order() { Id = id, InvestorId = Email, Type = OrderType.SELL, ShareName = SelectedSellingShare.ShareName, Limit = LowerPriceLimit, TotalNoOfShares = NoOfSharesSelling, NoOfProcessedShares = 0 };
            data.PlaceOrder(order);
        }

        private void CancelPendingOrder()
        {
            data.CancelOrder(SelectedPendingOrder);
        }

        private void Logout()
        {
            data.Dispose();
            App.Current.Shutdown();
        }
    }
}