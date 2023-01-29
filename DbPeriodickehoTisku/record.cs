using System;




namespace DbPeriodickehoTisku
{

    public class record
    {

        public string Id { get; set; }
         
        public string EvidencniCislo { get; set; }
        public string Periodikum { get; set; }
        public vydavatel Vydavatel { get; set; }
        public string Zamereni { get; set; }
        public string Okres { get; set; }
        public string Kraj { get; set; }
        public int Periodicita { get; set; }

        public DateTime? Evidovano { get; set; }
        
        public DateTime? Preruseno { get; set; }
        
        public DateTime? Ukonceno { get; set; }

        public class vydavatel
        {
            public string Jmeno { get; set; }
            public string ICO { get; set; }
        }
    }
}


