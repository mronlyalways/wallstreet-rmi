using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Investor.localhost;
using Investor.Model;
using Investor.View;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Investor.ViewModel
{

    public class LoginViewModel : ViewModelBase
    {
        private IDataService data;

        public LoginViewModel(IDataService data)
        {
            this.data = data;
            var exchanges = data.LoadExchangeInformation();
            SubmitCommand = new RelayCommand(Submit, () => true);
            Email = string.Empty;
            Registrations = new ObservableCollection<RegistrationInfo>(exchanges.Select(x => new RegistrationInfo { ExchangeName = x, Budget = 0.0}));
            ButtonText = "Submit";
        }

        private string email;
        public string Email
        {
            get
            {
                return email;
            }
            set
            {
                email = value;
                RaisePropertyChanged(() => Email);
                SubmitCommand.RaiseCanExecuteChanged();
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
                SubmitCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand SubmitCommand { get; private set; }

        public void Submit()
        {
            ButtonText = "Waiting for confirmation ...";
            foreach (RegistrationInfo i in Registrations.Where(x => x.Register))
            {
                data.Login(new InvestorRegistration() { Email = Email, Budget = i.Budget }, i.ExchangeName);
            }
            ((ViewModelLocator)App.Current.Resources["Locator"]).RegisteredExchanges = Registrations.Where(x => x.Register).Select(x => x.ExchangeName);
            Messenger.Default.Send<NotificationMessage>(new NotificationMessage(this, "Close"));
            var MainWindow = new MainWindow();
            MainWindow.Show();
            this.Cleanup();
        }
    }
}