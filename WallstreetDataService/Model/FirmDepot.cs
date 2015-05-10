using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WallstreetDataService.Model
{
    [DataContract]
    public class FirmDepot
    {
        [DataMember]
        public string FirmName { get; set; }

        [DataMember]
        public int OwnedShares { get; set; }
    }
}
