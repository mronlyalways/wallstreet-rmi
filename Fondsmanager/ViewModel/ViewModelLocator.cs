using Ninject;
using FundManager.Model;
using FundManager.localhost;
using System.Collections.Generic;

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

        public IList<FundDepot> Funds { get; set; }

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