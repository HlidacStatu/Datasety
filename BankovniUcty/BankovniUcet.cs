using System;

namespace BankovniUcty
{
    public class BankovniUcet : HlidacStatu.Api.Dataset.Connector.IDatasetItem
    {
        public string Id { get; set; }
        public string Subjekt { get; set; }
        public string Nazev { get; set; }
        public string Mena { get; set; }
        public string TypSubjektu { get; set; }
        public string Url{ get; set; }
        public string CisloUctu { get; set; }
        public DateTime LastSuccessfullParsing { get; set; }
        public int Active { get; set; }
        public string TypUctu { get; set; }  //0 = neznámý, 1 = provozní, 2 = volební
    }
}
