using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Investor.localhost;
using Investor.Model;
using Investor.View;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Investor.ViewModel
{

    public class LoginViewModel : ViewModelBase
    {
        private IDataService data;

        public LoginViewModel(IDataService data)
        {
            this.data = data;
            SubmitCommand = new RelayCommand(Submit, () => !Email.Equals(string.Empty) && Budget >= 0);
            Email = string.Empty;
            Budget = 0;
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

        private double budget;
        public double Budget
        {
            get
            {
                return budget;
            }
            set
            {
                budget = value;
                RaisePropertyChanged(() => Budget);
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
                SubmitCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand SubmitCommand { get; private set; }

        public void Submit()
        {
            ButtonText = "Waiting for confirmation ...";
            data.Login(new InvestorRegistration() { Email = Email, Budget = Budget });
            Messenger.Default.Send<NotificationMessage>(new NotificationMessage(this, "Close"));
            var MainWindow = new MainWindow();
            MainWindow.Show();
            this.Cleanup();
        }
    }
}