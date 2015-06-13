using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using FundManager.Model;
using FundManager.View;
using FundManager.localhost;

namespace FundManager.ViewModel
{

    public class LoginViewModel : ViewModelBase
    {
        private IDataService data;
        private bool submitted;

        public LoginViewModel(IDataService data)
        {
            this.data = data;
            data.AddNewFundInformationAvailableCallback(OnNewFundDepotAvailable);
            SubmitCommand = new RelayCommand(Submit, () => !FundID.Equals(string.Empty) && FundAssests >= 0 && FundShares >= 0 && !submitted);
            FundID = string.Empty;
            FundAssests = 0;
            FundShares = 0;
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

        private double fundassets;
        public double FundAssests
        {
            get
            {
                return fundassets;
            }
            set
            {
                fundassets = value;
                RaisePropertyChanged(() => FundAssests);
                SubmitCommand.RaiseCanExecuteChanged();
            }
        }

        private int fundshares;

        public int FundShares
        {
            get
            {
                return fundshares;
            }
            set
            {
                fundshares = value;
                RaisePropertyChanged(() => FundShares);
                SubmitCommand.RaiseCanExecuteChanged();
            }
        }

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
            data.Login(new FundRegistration() { Id = FundID, FundAssets = FundAssests, Shares = FundShares });
            submitted = true;
            ButtonText = "Waiting for confirmation ...";
        }

        public void OnNewFundDepotAvailable(FundDepot depot)
        {
            Messenger.Default.Send<NotificationMessage>(new NotificationMessage(this, "Close"));
            var MainWindow = new MainWindow();
            MainWindow.Show();
            this.Cleanup();
        }
    }
}