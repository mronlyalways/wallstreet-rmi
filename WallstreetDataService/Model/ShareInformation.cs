using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WallstreetDataService.Model
{
    [DataContract]
    public class ShareInformation
    {
        [DataMember]
        public string ExchangeName { get; set; }

        [DataMember]
        public string FirmName { get; set; }

        [DataMember]
        public int NoOfShares { get; set; }

        [DataMember]
        public int PurchasingVolume { get; set; }

        [DataMember]
        public int SalesVolume { get; set; }

        [DataMember]
        public double PricePerShare { get; set; }

        [DataMember]
        public bool IsFund { get; set; }
    }
}
