using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WallstreetDataService.Model
{
    [DataContract]
    public class Transaction
    {
        [DataMember]
        public string TransactionId { get; set; }

        [DataMember]
        public long BrokerId { get; set; }

        [DataMember]
        public string ShareName { get; set; }

        [DataMember]
        public string BuyingOrderId { get; set; }

        [DataMember]
        public string SellingOrderId { get; set; }

        [DataMember]
        public string BuyerId { get; set; }

        [DataMember]
        public string SellerId { get; set; }

        [DataMember]
        public double PricePerShare { get; set; }

        [DataMember]
        public int NoOfSharesSold { get; set; }

        [DataMember]
        public double TotalCost
        {
            get
            {
                return PricePerShare * NoOfSharesSold;
            }
            internal set { }
        }

        [DataMember]
        public double Provision
        {
            get
            {
                return TotalCost * 0.03;
            }
            internal set { }
        }
    }
}
