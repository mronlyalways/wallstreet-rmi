using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace WallstreetDataService.Model
{
    [DataContract]
    public class FundRequestResult : RequestResult
    {
        [DataMember]
        public FundDepot FundDepot { get; set; }
    }
}
