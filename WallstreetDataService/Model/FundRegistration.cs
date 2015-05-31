using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WallstreetDataService.Model
{
    [DataContract]
    public class FundRegistration
    {
        [DataMember]
        public String FundID
        {
            get;
            set;
        }

        [DataMember]
        public double FundAssets
        {
            get;
            set;
        }

        [DataMember]
        public int FundShares
        {
            get;
            set;
        }
    }
}