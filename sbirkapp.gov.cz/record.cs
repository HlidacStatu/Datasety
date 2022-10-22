using System;

using CsvHelper.Configuration.Attributes;

namespace sbirkapp.gov.cz
{

    public class record
    {

        [Name("Čas nabytí účinnosti")]
        public string CasNabytiUcinnosti { get; set; }
        [Name("Číslo")] public string Cislo { get; set; }
        [Name("Datum nabytí účinnosti")]
        public DateTime? DatumNabytiUcinnosti { get; set; }
        [Name("Datum schválení")]
        public DateTime? DatumSchvaleni { get; set; }
        [Name("Datum vyvěšení na úřední desce")]
        public DateTime? DatumVyveseniNaUredniDesce { get; set; }
        [Name("Datum zveřejnění")]
        public DateTime? DatumZverejneni { get; set; }
        [Name("Druh")]
        public string Druh { get; set; }

        [Name("ID záznamu")]
        public string Id { get; set; }
        [Name("Kraj")]
        public string Kraj { get; set; }
        [Name("Kraj vydavatele")]
        public string KrajVydavatele { get; set; }
        [Name("Název")]
        public string Nazev { get; set; }
        [Name("Oblast právní úpravy")]
        public string OblastPravniUpravy { get; set; }
        [Name("Počet členú zastupitelstva")]
        public string PocetClenuZastupitelstva { get; set; }
        [Name("Právní zmocnění")]
        public string PravniZmocneni { get; set; }

        [Name("Rok, v němž se konají volby")]
        public string RokVNemzSeKonajiVolby { get; set; }
        [Name("Spisová značka")]
        public string SpisovaZnacka { get; set; }
        [Name("Spisová značka Ústavního soudu")]
        public string SpisovaZnackaUstavnihoSoudu { get; set; }
        [Name("Typ rozhodnutí")]
        public string TypRozhodnuti { get; set; }
        [Name("Typ smlouvy")]
        public string TypSmlouvy { get; set; }
        [Name("Usnesení")]
        public string Usneseni { get; set; }

        [Name("Území, na kterém je stav nebezpečí vyhlášen")]
        public string UzemiNaKteremJeStavNebezpeciVyhlasen { get; set; }
        [Name("Území, na kterém se chráněné území nachází")]
        public string UzemiNaKteremSeChraneneUzemiNachazi { get; set; }
        [Name("Verze")]
        public double? Verze { get; set; }
        [Name("Vydavatel")]
        public string Vydavatel { get; set; }
        [Name("Vydavatel ID DS")]
        public string VydavatelIdDs { get; set; }

        [Name("Vydavatel ID OVM")]
        public string ICO { get; set; }
        [Name("Způsob zveřejnění")]
        public string ZpusobZverejneni { get; set; }

        [CsvHelper.Configuration.Attributes.Ignore]
        public doc TextPredpisu { get; set; }

        [CsvHelper.Configuration.Attributes.Ignore]
        public doc[] PrilohyPredpisu { get; set; }
        public class doc
        {
            public string HsProcessType { get; set; } = "document";
            public string DocumentPlainText { get; set; }
            public string DocumentUrl { get; set; }
            public string DocumentName { get; set; }
        }
    }
}


