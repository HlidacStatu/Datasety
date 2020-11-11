using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace deMinimis
{
    public partial class JednoduchaPodpora
    {
        static Response.Ciselnik[] formy = deMinimis.DeMinimisCalls.Ciselnik("c_forma_podpory");
        static Response.Ciselnik[] priznaky_podpory = deMinimis.DeMinimisCalls.Ciselnik("c_priznaky_podpory");
        static Response.Ciselnik[] stav_podpory = deMinimis.DeMinimisCalls.Ciselnik("c_stav_podpory");
        static Response.Ciselnik[] duvod_zruseni_podpory = deMinimis.DeMinimisCalls.Ciselnik("c_duvod_zruseni_podpory");
        static Response.Ciselnik[] pravni_akt_poskytnuti = deMinimis.DeMinimisCalls.Ciselnik("c_pravni_akt_poskytnuti");

        public static JednoduchaPodpora FromJson(string zaznam)
        {
            if (string.IsNullOrWhiteSpace(zaznam))
                return null;
            if (!zaznam.Trim().StartsWith("{"))
                return null;
            zaznam = zaznam.Trim();
            if (zaznam.EndsWith(","))
                zaznam = zaznam.Remove(zaznam.Length - 1);

            OldCl i = Newtonsoft.Json.JsonConvert.DeserializeObject<OldCl>(zaznam);

            JednoduchaPodpora p = new JednoduchaPodpora();
            //p.CjPrijemce = i.;
            p.Edidat = i.Edidat;
            p.Ico = i.Ico.ToString().PadLeft(8, '0');
            p.Id = i.Id;
            p.Insdat = i.Insdat;
            p.PodporaCzk = (decimal)i.PodporaCzk;
            p.PodporaDatum = i.PodporaDatum;
            p.PodporaEur = (decimal)i.PodporaEur;
            p.PodporaForma_Kod = i.PodporaFormaKod;
            p.PodporaFormaText = i.PodporaFormaText;
            p.PodporaUcel = i.PodporaUcel;
            p.PoskytovatelIco = i.PoskytovatelOjm;
            p.PoskytovatelOjm = i.PoskytovatelOjm;
            p.Poskytovatel_SZRId = i.OrgSzrid;
            p.PravniAktPoskytnutiId = i.PravniAktPoskytnutiId;
            p.PravniAktPoskytnutiText = i.PravniAktPoskytnutiText;
            p.ProjektId = i.ProjektId;

            p.StavKod = i.StavKod;
            p.StavKodText = i.StavKodText;
            p.Prijemce_SZRId = i.Subjektid;
            return p;
        }
        public static List<JednoduchaPodpora> FromAPI(deMinimis.Response.Detail.Response zaznam)
        {
            if (zaznam == null)
                return null;
            if (zaznam.seznam_subjektu == null)
                return null;

            if (zaznam.seznam_subjektu?.Length < 1)
                return null;

            List<JednoduchaPodpora> podpory = new List<JednoduchaPodpora>();
            foreach (var s in zaznam.seznam_subjektu)
            {
                foreach (var i in s.seznam_podpor)
                {
                    JednoduchaPodpora p = new JednoduchaPodpora();
                    p.CjPrijemce = i.cj_prijemce;
                    p.Edidat = i.edidat;
                    p.Ico = s.seznam_identifikatoru.Where(m => m.typ == "ICO").OrderByDescending(o => o.platnost_od).FirstOrDefault()?.Value;
                    p.JmenoPrijemce = s.obchodni_jmeno;
                    p.Id = i.id_podpory;
                    p.Insdat = i.insdat;
                    p.PodporaCzk = i.castka_kc;
                    p.PodporaDatum = i.datum_prideleni;
                    p.PodporaEur = i.castka_euro;
                    p.PodporaForma_Kod = i.forma_podpory;
                    p.PodporaFormaText = formy.FirstOrDefault(m => m.kod == i.forma_podpory)?.nazev;
                    p.PodporaUcel = i.ucel_podpory;
                    if (i.poskytovatel_ic != 0)
                        p.PoskytovatelIco = i.poskytovatel_ic.ToString("00000000");

                    p.PoskytovatelOjm = i.poskytovatel_ojm;
                    p.Poskytovatel_SZRId = i.poskytovatel_id;
                    p.PravniAktPoskytnutiId = i.pravni_akt_poskytnuti;
                    p.PravniAktPoskytnutiText = pravni_akt_poskytnuti.FirstOrDefault(m => m.kod == i.pravni_akt_poskytnuti)?.nazev;
                    p.ProjektId = i.cj_poskytovatel;

                    p.RezimPodporyId = i.rezim_podpory.adhoc ? "Adhoc podpora" : (string.IsNullOrEmpty(i.rezim_podpory.nazev) ? "režim podpory" : i.rezim_podpory.nazev);
                    p.StavKod = i.stav;
                    p.StavKodText = stav_podpory.FirstOrDefault(m => m.kod == i.stav)?.nazev;
                    p.Prijemce_SZRId = s.subjektid;
                    podpory.Add(p);
                }
            }
            return podpory;
        }

        public JednoduchaPodpora() { }

        [Description("ID podpory")]
        public int Id { get; set; }
        [Description("Unikatni ID příjemce v systému SZR")]
        public int Prijemce_SZRId { get; set; }
        [Description("IČO příjemce")]
        public string Ico { get; set; }
        [Description("Obchodní jméno či jméno příjemce")]
        public string JmenoPrijemce { get; set; }
        [Description("Kód oblasti v RDM")]
        public string RdmOblastKod { get; set; }
        [Description("Výše podpory v českých korunách")]
        public decimal PodporaCzk { get; set; }
        [Description("Výše podory v Eurech")]
        public decimal PodporaEur { get; set; }
        [Description("Datum schválení podpory")]
        public DateTime PodporaDatum { get; set; }
        [Description("Kód formy podpory")]
        public int PodporaForma_Kod { get; set; }
        [Description("Textový popis formy podpory")]
        public string PodporaFormaText { get; set; }
        [Description("Účel podpory")]
        public string PodporaUcel { get; set; }
        [Description("ID projektu, pokud je uvedeno")]
        public string ProjektId { get; set; }
        [Description("ID projektu u příjemce")]
        public string CjPrijemce { get; set; }
        [Description("Stav podpory")]
        public int StavKod { get; set; }
        [Description("Textový popis stavu podpory")]
        public string StavKodText { get; set; }
        [Description("Datum vložení záznamu do registru de minimis")]
        public DateTime? Insdat { get; set; }
        [Description("Datum poslední editace záznamu v registru de minimis")]
        public DateTime? Edidat { get; set; }
        [Description("ID poskytovatele podpory v systému SZR")]
        public int Poskytovatel_SZRId { get; set; }
        [Description("Jméno poskytovatele podpory")]
        public string PoskytovatelOjm { get; set; }
        [Description("IČO poskytovatele podpory")]
        public string PoskytovatelIco { get; set; }
        [Description("Kód právního aktu, na základě kterého došlo k poskytnutí podpory")]
        public int PravniAktPoskytnutiId { get; set; }
        [Description("Právní akt, na základě kterého došlo k poskytnutí podpory")]
        public string PravniAktPoskytnutiText { get; set; }
        [Description("Režim podpory")]
        public string RezimPodporyId { get; set; }
    }
}