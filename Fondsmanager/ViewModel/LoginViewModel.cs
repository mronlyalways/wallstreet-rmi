using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using FundManager.Model;
using FundManager.View;
using FundManager.localhost;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;

namespace FundManager.ViewModel
{

    public class LoginViewModel : ViewModelBase
    {
        private IDataService data;
        private ViewModelLocator locator;
        private bool submitted;
        private int confirmedCounter;

        public LoginViewModel(IDataService data)
        {
            this.data = data;
            this.locator = (ViewModelLocator) App.Current.Resources["Locator"];
            confirmedCounter = -1;
            data.AddNewFundInformationAvailableCallback(OnNewFundDepotAvailable);
            SubmitCommand = new RelayCommand(Submit, () => !FundID.Equals(string.Empty) && !submitted);
            FundID = string.Empty;
            Exchanges = new ObservableCollection<string>(data.LoadExchangeInformation());
            Registrations = new ObservableCollection<RegistrationInfo>(Exchanges.Select(x => new RegistrationInfo { ExchangeName = x, Budget = 0, Register = false }));
            ButtonText = "Submit";
            submitted = false;
        }

        private string fundid;
        public string FundID
        {
            get
            {
                return fundid;
            }
            set
            {
                fundid = value;
                RaisePropertyChanged(() => FundID);
                SubmitCommand.RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<string> Exchanges { get; set; }

        private string mainExchange;
        public string MainExchange
        {
            get
            {
                return mainExchange;
            }
            set
            {
                mainExchange = value;
                RaisePropertyChanged(() => MainExchange);
            }
        }

        private double mainBudget;
        public double MainBudget
        {
            get
            {
                return mainBudget;
            }
            set
            {
                mainBudget = value;
                RaisePropertyChanged(() => MainBudget);
            }
        }

        private int mainShares;
        public int MainShares
        {
            get
            {
                return mainShares;
            }
            set
            {
                mainShares = value;
                RaisePropertyChanged(() => MainShares);
            }
        }

        public ObservableCollection<RegistrationInfo> Registrations { get; set; }

        private string buttonText;
        public string ButtonText
        {
            get
            {
                return buttonText;
            }
            set
            {
                buttonText = value;
                RaisePropertyChanged(() => ButtonText);
            }
        }

        public RelayCommand SubmitCommand { get; private set; }

        public void Submit()
        {
            submitted = true;
            data.Login(new FundRegistration { Id = FundID, FundAssets = MainBudget, Shares = MainShares }, MainExchange);
            
            foreach (RegistrationInfo i in Registrations.Where(x => x.Register))
            {
                data.Login(new FundRegistration { Id = FundID, FundAssets = i.Budget, Shares = 0 }, i.ExchangeName);
            }
            ButtonText = "Waiting for confirmation ...";
        }

        public void OnNewFundDepotAvailable(FundDepot depot)
        {
            if (locator.Funds == null)
            {
                locator.Funds = new List<FundDepot>();
            }
            locator.Funds.Add(depot);
            confirmedCounter++;
            if (confirmedCounter == Registrations.Where(x => x.Register).Count())
            {
                Messenger.Default.Send<NotificationMessage>(new NotificationMessage(this, "Close"));
                var MainWindow = new MainWindow();
                MainWindow.Show();
                this.Cleanup();
            }
        }
    }
}