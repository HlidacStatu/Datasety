using System;

namespace BankovniUcty
{
    class BankovniPolozka : HlidacStatu.Api.Dataset.Connector.IDatasetItem
    {
        public string Id { get; set; }
        public string CisloUctu { get; set; }
        public DateTime Datum { get; set; }
        public string PopisTransakce { get; set; }
        public string NazevProtiuctu { get; set; }
        public string CisloProtiuctu { get; set; }
        public string Zprava { get; set; }
        public string VS { get; set; }
        public string KS { get; set; }
        public string SS { get; set; }
        public decimal Castka { get; set; }
        public string ZdrojUrl { get; set; }
    }
}
