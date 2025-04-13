using System;
using System.Linq;

namespace RejstrikTrestuPravnickychOsob
{
    public class Trest
    {
        static System.Text.RegularExpressions.RegexOptions options = ((System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace | System.Text.RegularExpressions.RegexOptions.Multiline)
            | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        System.Text.RegularExpressions.Regex regZakon = new System.Text.RegularExpressions.Regex("(?<cislo>\\d*([a-z]?))/(?<rok>\\d*)", options);


        public Trest() { }

        public Trest(VypisXML.Vypis xml)
        {
            try
            {

                this.ICO = xml.osobaPravnicka.osobaPravnickaCeska.ico.ToString("00000000");
                this.NazevFirmy = xml.osobaPravnicka.osobaPravnickaCeska.nazev;
                this.Sidlo = xml.osobaPravnicka.osobaPravnickaCeska.sidlo;
                this.RUIANCode = xml.osobaPravnicka.osobaPravnickaCeska.sidloRuianCode.ToString();

                if (xml.zaznamy.zaznam != null)
                {
                    this.DatumPravniMoci = (DateTime)xml.zaznamy.zaznam.odsouzeni.Items[
                        xml.zaznamy.zaznam.odsouzeni.ItemsElementName.ToList().IndexOf(VypisXML.ItemsChoiceType.datumPM)
                        ];
                    this.Odsouzeni = new odsouzeni();
                    this.Odsouzeni.PrvniInstance = new soud();
                    this.Odsouzeni.PrvniInstance.DatumRozhodnuti = (DateTime)xml.zaznamy.zaznam.odsouzeni.Items[
                        xml.zaznamy.zaznam.odsouzeni.ItemsElementName.ToList().IndexOf(VypisXML.ItemsChoiceType.datumRozhodnuti)
                        ];
                    this.Odsouzeni.PrvniInstance.DruhRozhodnuti = ((RejstrikTrestuPravnickychOsob.VypisXML.vypisListVypisZaznamyZaznamOdsouzeniDruhRozhodnuti)xml.zaznamy.zaznam.odsouzeni.Items[
                        xml.zaznamy.zaznam.odsouzeni.ItemsElementName.ToList().IndexOf(VypisXML.ItemsChoiceType.druhRozhodnuti)
                        ]).druh;
                    this.Odsouzeni.PrvniInstance.Jmeno = (string)xml.zaznamy.zaznam.odsouzeni.Items[
                        xml.zaznamy.zaznam.odsouzeni.ItemsElementName.ToList().IndexOf(VypisXML.ItemsChoiceType.organizace)
                        ];
                    this.Odsouzeni.PrvniInstance.SpisovaZnacka = (string)xml.zaznamy.zaznam.odsouzeni.Items[
                        xml.zaznamy.zaznam.odsouzeni.ItemsElementName.ToList().IndexOf(VypisXML.ItemsChoiceType.spisZnacka)
                        ];

                    if (xml.zaznamy.zaznam.odsouzeni.ItemsElementName.ToList().IndexOf(VypisXML.ItemsChoiceType.odvolaci) >= 0)
                    {
                        RejstrikTrestuPravnickychOsob.VypisXML.vypisListVypisZaznamyZaznamOdsouzeniOdvolaci odvol =
                            (RejstrikTrestuPravnickychOsob.VypisXML.vypisListVypisZaznamyZaznamOdsouzeniOdvolaci)
                            xml.zaznamy.zaznam.odsouzeni.Items[
                                xml.zaznamy.zaznam.odsouzeni.ItemsElementName.ToList().IndexOf(VypisXML.ItemsChoiceType.odvolaci)
                                ];
                        this.Odsouzeni.OdvolaciSoud = new soud();
                        this.Odsouzeni.OdvolaciSoud.DatumRozhodnuti = odvol.datumRozhodnuti;
                        this.Odsouzeni.OdvolaciSoud.DruhRozhodnuti = odvol.druhRozhodnuti.druh;
                        this.Odsouzeni.OdvolaciSoud.Jmeno = odvol.organizace;
                        this.Odsouzeni.OdvolaciSoud.SpisovaZnacka = odvol.spisZnacka;
                    }

                    if (xml.zaznamy.zaznam.paragrafy != null)
                    {
                        this.Paragrafy = xml.zaznamy.zaznam.paragrafy
                            .Where(m => m.paragrafCisel != null)
                            .Select(m => new paragraf()
                            {
                                Zakon = new paragraf.zakon()
                                {
                                    Rok = Convert.ToInt32(regZakon.Match(m.paragrafCisel.zakon.zkratka).Groups["rok"]?.Value),
                                    ZakonCislo = regZakon.Match(m.paragrafCisel.zakon.zkratka).Groups["cislo"].Value,
                                    OdstavecPismeno = m.paragrafCisel.odstavecPismeno,
                                    ParagrafCislo = m.paragrafCisel.paragrafCislo
                                },
                                ZakonPopis = m.paragrafCisel.zakon.Value,
                                Zavineni = m.paragrafCisel.zavineni?.typ,

                                Doplneni = (m.paragrafCisel.paragrafyDoplnek != null && m.paragrafCisel.paragrafyDoplnek.Length > 0) ?
                                            m.paragrafCisel.paragrafyDoplnek.Select(p => p.ToString()).Aggregate((f, s) => f + ", " + s)
                                            : null
                            })
                            .ToArray();
                    }
                    else
                    {
                        var x = 1;
                    }
                    if (xml.zaznamy.zaznam.tresty != null)
                    {
                        this.Tresty = xml.zaznamy.zaznam.tresty
                            .Select(m =>
                                {
                                    var druh = m.Items.FirstOrDefault(d => d.GetType() == typeof(RejstrikTrestuPravnickychOsob.VypisXML.druh)) as RejstrikTrestuPravnickychOsob.VypisXML.druh;
                                    trest t = null;
                                    if (druh != null)
                                    {
                                        t = new trest()
                                        {
                                            Druh = druh.zkratka,
                                            DruhText = druh.Value,
                                        };
                                    }
                                    var vymery = m.Items.FirstOrDefault(d => d.GetType() == typeof(RejstrikTrestuPravnickychOsob.VypisXML.vymery)) as RejstrikTrestuPravnickychOsob.VypisXML.vymery;
                                    if (vymery != null)
                                        t.vymery = vymery.vymera?.Select(m => new vymera()
                                        {
                                            PolozkaText = m.polozka?.Value,
                                            PolozkaZkratka = m.polozka?.zkratka,
                                            SkupinaText = m.skupina?.Value,
                                            SkupinaZkratka = m.skupina?.zkratka,
                                            Hodnota = m.hodnota,
                                        }).ToArray();

                                    var prubehy = m.Items.FirstOrDefault(d => d.GetType() == typeof(RejstrikTrestuPravnickychOsob.VypisXML.prubehy)) as RejstrikTrestuPravnickychOsob.VypisXML.prubehy;
                                    if (prubehy != null)
                                        t.prubehy = prubehy.prubeh?.Select(m => new prubeh()
                                        {
                                            PolozkaText = m.polozka?.Value,
                                            PolozkaZkratka = m.polozka?.zkratka,
                                            SkupinaText = m.skupina?.Value,
                                            SkupinaZkratka = m.skupina?.zkratka,
                                            Hodnota = m.hodnota,
                                        }).ToArray();
                                    return t;
                                }
                            )
                            .Where(m => m != null)
                            .ToArray();
                    }

                }

            }
            catch (Exception e)
            {

                throw;
            }

        }

        public string Id { get { return $"{this.ICO}-{this.DatumPravniMoci:yyyyMMdd}"; } }
        public string ICO { get; set; }
        public string NazevFirmy { get; set; }
        public string Sidlo { get; set; }
        public string RUIANCode { get; set; }
        public DateTime DatumPravniMoci { get; set; }

        public odsouzeni Odsouzeni { get; set; }
        public class odsouzeni
        {
            public soud PrvniInstance { get; set; }
            public soud OdvolaciSoud { get; set; }
        }
        public paragraf[] Paragrafy { get; set; }

        public trest[] Tresty { get; set; }

        public class trest
        {
            public string Druh { get; set; }
            public string DruhText { get; set; }

            public vymera[] vymery { get; set; }
            public prubeh[] prubehy { get; set; }

        }
        public class vymera
        {
            public string SkupinaZkratka { get; set; }
            public string SkupinaText { get; set; }

            public string PolozkaZkratka { get; set; }
            public string PolozkaText { get; set; }

            public string Hodnota { get; set; }
        }

        public class prubeh
        {
            public string SkupinaZkratka { get; set; }
            public string SkupinaText { get; set; }

            public string PolozkaZkratka { get; set; }
            public string PolozkaText { get; set; }

            public string Hodnota { get; set; }
        }

        public class ZkratkaText
        {
            public string Zkratka { get; set; }
            public string Text { get; set; }

        }

        public class soud
        {
            public string DruhRozhodnuti { get; set; }
            public DateTime DatumRozhodnuti { get; set; }
            public string Jmeno { get; set; }
            public string SpisovaZnacka { get; set; }
        }

        public class paragraf
        {
            public zakon Zakon { get; set; }
            public class zakon
            {
                public int Rok { get; set; }
                public string ZakonCislo { get; set; }
                public string ParagrafCislo { get; set; }
                public string OdstavecPismeno { get; set; }
            }
            public string ZakonPopis { get; set; }
            public string Zavineni { get; set; }
            public string Doplneni { get; set; }
        }



        public string TextOdsouzeni { get; set; }
    }


}
