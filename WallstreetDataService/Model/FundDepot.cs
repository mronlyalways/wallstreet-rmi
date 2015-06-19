using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WallstreetDataService.Model
{
    [DataContract]
    [KnownType(typeof(InvestorDepot))]
    public class FundDepot : InvestorDepot
    {
    }
}