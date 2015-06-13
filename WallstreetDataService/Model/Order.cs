using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WallstreetDataService.Model
{
    [DataContract]
    public class Order
    {
        [DataContract(Name = "OrderStatus")]
        public enum OrderStatus
        { 
            [EnumMember]
            OPEN,
            [EnumMember]
            PARTIAL,
            [EnumMember]
            DONE,
            [EnumMember]
            DELETED
        }

        [DataContract(Name = "OrderType")]
        public enum OrderType
        {
            [EnumMember]
            BUY,
            [EnumMember]
            SELL
        }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string InvestorId { get; set; }

        [DataMember]
        public OrderType Type { get; set; }

        [DataMember]
        public string ShareName { get; set; }

        [DataMember]
        public double Limit { get; set; }

        [DataMember]
        public int TotalNoOfShares { get; set; }

        [DataMember]
        public int NoOfProcessedShares { get; set; }

        [DataMember]
        public bool Prioritize { get; set; }

        [DataMember]
        public bool IsFundShare { get; set; }

        [DataMember]
        public int NoOfOpenShares
        {
            get
            {
                return TotalNoOfShares - NoOfProcessedShares;
            }
            internal set { }
        }

        [DataMember]
        public OrderStatus Status { get; set; }

        //public override bool Equals(object obj)
        //{
        //    var other = obj as Order;
        //    if (other != null)
        //    {
        //        var result = Id.Equals(other.Id) 
        //            && InvestorId.Equals(other.InvestorId) 
        //            && Type == other.Type 
        //            && ShareName.Equals(other.ShareName) 
        //            && Limit == other.Limit 
        //            && TotalNoOfShares == other.TotalNoOfShares 
        //            && Status == other.Status;
        //        return result;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        public override string ToString()
        {
            return base.ToString() + " Id:" + Id + " Status:" + Status + " InvestorId:" + InvestorId + " Type:" + Type + " ShareName:" + ShareName + " Limit:" + Limit + " TotalNoOfShares:" + TotalNoOfShares;
        }

        //public override int GetHashCode()
        //{
        //    return TotalNoOfShares * NoOfProcessedShares;
        //}
    }
}
