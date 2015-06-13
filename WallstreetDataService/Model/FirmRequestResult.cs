using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WallstreetDataService.Model
{
    [DataContract]
    public class FirmRequestResult : RequestResult
    {
        [DataMember]
        public FirmDepot FirmDepot { get; set; }
    }
}