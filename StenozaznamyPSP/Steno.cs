using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StenozaznamyPSP
{
    public class Steno : HlidacStatu.Api.Dataset.Connector.IDatasetItem
    {
        //csv rok,datum,schuze,fn,autor,funkce,tema,text
        public string CalcId(int row)
        {
            return $"{obdobi}_{schuze}_{row:00000}";
        }
        public int GetRowFromId()
        {
            var p = this.Id.Split('_');
            if (p.Length == 3)
                return Convert.ToInt32(p[2]);
            else
                return 0;
        }

        public string Id { get; set; }
        public int obdobi { get; set; }
        public DateTime? datum { get; set; }
        public int schuze { get; set; }
        public string fn { get; set; }
        public string celeJmeno { get; set; }
        public DateTime? narozeni { get; set; } 
        public string HsProcessType { get; set; } = "person";
        public string OsobaId { get; set; }
        public string funkce { get; set; }
        public string tema { get; set; }
        public string text { get; set; }
    }
}
