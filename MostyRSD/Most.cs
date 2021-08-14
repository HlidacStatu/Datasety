using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MostyRSD
{
    public class Most 
    {
        public string Id { get; set; }
        public string Jmeno { get; set; }
        public string MistniNazev { get; set; }
        public string ProhlidkaPopis { get; set; }
        public DateTime? PosledniProhlidka { get; set; }
        public string Oznaceni { get; set; }
        public int Stav { get; set; }
        public string PopisStavu { get; set; }
        public string SpravaOrganizace { get; set; }
        public string SpravaProvozniUsek { get; set; }
        public string SpravaStredisko { get; set; }
        public double GPS_Lat { get; set; }
        public double GPS_Lng { get; set; }
    }
}
