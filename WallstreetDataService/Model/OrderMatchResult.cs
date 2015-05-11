using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WallstreetDataService.Model
{
    [DataContract]
    public class OrderMatchResult
    {
        [DataMember]
        public Order Order { get; set; }
        
        [DataMember]
        public IEnumerable<Order> Matches { get; set; }

        [DataMember]
        public IEnumerable<Transaction> Transactions { get; set; }
    }
}