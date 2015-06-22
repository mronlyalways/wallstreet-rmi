using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WallstreetDataService.Model
{
    [DataContract]
    [KnownType(typeof(FundDepot))]
    public class InvestorDepot
    {
        [DataMember]
        public string ExchangeName { get; set; }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public double Budget { get; set; }

        [DataMember]
        public Dictionary<string, int> Shares { get; set; }
    }
}
