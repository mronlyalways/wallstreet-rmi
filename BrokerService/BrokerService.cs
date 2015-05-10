using BrokerService.localhost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace BrokerService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class BrokerService : IBrokerService
    {
        private WallstreetDataServiceClient client;
        private IList<Func<Request, Tuple<FirmDepot, ShareInformation, Order>>> brokers;

        public BrokerService()
        {
            client = new WallstreetDataServiceClient(new InstanceContext(this));
        }

        public void Register()
        {
            var subscriber = OperationContext.Current.GetCallbackChannel<IBroker>();
            brokers.Add(subscriber.OnNewRegistrationRequestAvailable);
        }

        Tuple<FirmDepot, ShareInformation, Order> RegisterFirm(Request request)
        {
            if (brokers.Count > 0)
            {
                return brokers[0](request);
            }
            else
            {
                throw new NoBrokerOnlineException();
            }
        }

        private void CallWithArgument<T>(IEnumerable<Action<T>> callbacks, T arg)
        {
            foreach (Action<T> callback in callbacks)
            {
                callback(arg);
            }
        }
    }
}
