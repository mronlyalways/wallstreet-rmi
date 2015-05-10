using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WallstreetDataService.Model
{
    [DataContract]
    public class Request
    {
        [DataMember]
        public String FirmName { get; set; }

        [DataMember]
        public int Shares { get; set; }

        [DataMember]
        public double PricePerShare { get; set; }
    }
}
