using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WallstreetDataService.Model
{
    [DataContract]
    [KnownType(typeof(FirmDepot))]
    [KnownType(typeof(Order))]
    [KnownType(typeof(ShareInformation))]
    public class FirmRequestResult
    {
        [DataMember]
        public FirmDepot FirmDepot { get; set; }

        [DataMember]
        public Order Order { get; set; }

        [DataMember]
        public ShareInformation ShareInformation { get; set; }
    }
}