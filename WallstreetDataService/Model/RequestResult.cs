using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace WallstreetDataService.Model
{
    [DataContract]
    public class RequestResult
    {
        [DataMember]
        public Order Order { get; set; }

        [DataMember]
        public ShareInformation ShareInformation { get; set; }
    }
}
