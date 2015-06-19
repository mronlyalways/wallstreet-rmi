using Ninject;
using FundManager.Model;
using FundManager.localhost;

namespace FundManager.ViewModel
{
    public class ViewModelLocator
    {
        private static StandardKernel kernel;

        static ViewModelLocator()
        {
            kernel = new StandardKernel();
            kernel.Bind<IDataService>().To<WcfDataService>().InSingletonScope();
        }

        public void SetFundDepot(FundDepot depot)
        {
            kernel.Bind<FundDepot>().ToConstant(depot);
        }

        public MainViewModel Main
        {
            get
            {
                return kernel.Get<MainViewModel>();
            }
        }

        public LoginViewModel Login
        {
            get
            {
                return kernel.Get<LoginViewModel>();
            }
        }

        /// <summary>
        /// Cleans up all the resources.
        /// </summary>
        public static void Cleanup()
        {
            kernel.Dispose();
        }
    }
}