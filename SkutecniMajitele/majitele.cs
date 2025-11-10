using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkutecniMajitele
{

    public class majitel_flat : majitel_base
    {
        public majitel_flat(xmlSubjekt subj, xmlSubjektUdajUdaj d)
            : base(d)
        {
            this.ico = subj.ico;
            this.nazev_subjektu = subj.nazev;
            string osobaId = $"{osoba_jmeno}-{osoba_prijmeni}-{osoba_datum_narozeni:yyyy-MM-dd}";
            this.id = this.ico + "-" + Devmasters.Crypto.Hash.ComputeHashToBase64(osobaId);

        }

        public string id { get; set; }
        public string ico { get; set; }
        public string nazev_subjektu { get; set; }

    }

    public class majitele
    {
        public void UpdateOsobaId()
        {
            if (this.skutecni_majitele != null)
            {
                for (int i = 0; i < this.skutecni_majitele.Count(); i++)
                {
                    this.skutecni_majitele[i].osobaId = GetOsobaId(this.skutecni_majitele[i]);

                }
            }
        }

        static DateTime minDate = new DateTime(1900, 1, 1);
        public static string GetOsobaId(majitel_base maj)
        {
            string osobaid = null;
            if (string.IsNullOrEmpty(maj.osoba_jmeno) && string.IsNullOrEmpty(maj.osoba_prijmeni))
                return null;
            if (string.IsNullOrEmpty(maj.osoba_jmeno) && !string.IsNullOrEmpty(maj.osoba_prijmeni))
            {
                var parts = maj.osoba_prijmeni.Split(' ');
                if (parts.Count() > 1)
                {
                    maj.osoba_jmeno = parts[0];
                    maj.osoba_prijmeni = string.Join(' ', parts.Skip(1));
                }
            }
            else
            {
                    maj.osoba_jmeno = maj.osoba_jmeno;
                    maj.osoba_prijmeni = maj.osoba_prijmeni;
            }

            if (maj.osoba_datum_narozeni < minDate)
                return null;

            try
            {
                maj.osobaId = findPerson.GetOsobaId(maj.osoba_jmeno, maj.osoba_prijmeni, maj.osoba_datum_narozeni);
            }
            catch (Exception)
            {
                System.Threading.Thread.Sleep(50);
                try
                {
                    maj.osobaId = findPerson.GetOsobaId(maj.osoba_jmeno, maj.osoba_prijmeni, maj.osoba_datum_narozeni);

                }
                catch (Exception e)
                {

                    Console.WriteLine(e);
                }
            }
            return osobaid;
        }

        public static majitele GetMajitele(xmlSubjekt subj)
        {
            majitele t = new majitele();
            if (Devmasters.TextUtil.IsNumeric(subj.ico))
                subj.ico = subj.ico.PadLeft(8, '0');
            t.ico = subj.ico;
            t.nazev_subjektu = subj.nazev;
            t.id = t.ico;

            var sekce = subj.udaje
                .Where(m => m.udajTyp.kod == "SKUTECNY_MAJITEL_SEKCE")
                .FirstOrDefault();
            if (sekce == null)
                return null;

            majitel_base[] _majitele = null;
            if (sekce.podudaje != null)
                _majitele = sekce.podudaje.Where(m => m?.udajTyp?.kod == "PRIMY_SKUTECNY_MAJITEL" || m?.udajTyp?.kod == "SKUTECNY_MAJITEL")
                   .Select(m => majitel_base.Get(m)).ToArray();
            if (_majitele?.Count() > 0)
            {
                t.skutecni_majitele = _majitele;
                return t;

            }
            return null;
        }
        private majitele() { }
        public string id { get; set; }
        public string ico { get; set; }
        public string nazev_subjektu { get; set; }
        public majitel_base[] skutecni_majitele { get; set; }

    }

    public class majitel_base
    {
        public static majitel_base Get(xmlSubjektUdajUdaj d)
        {
            var majitel = new majitel_base(d);

            return majitel;
        }




        protected majitel_base() { }
        protected majitel_base(xmlSubjektUdajUdaj d)
        {
            datum_zapis = d.zapisDatum;
            datum_vymaz = d.vymazDatumSpecified ? d.vymazDatum : null;
            udaj_typ = d.udajTyp.kod;
            udaj_typ_nazev = d.udajTyp.nazev;
            specifikace = d.hodnotaUdaje.specifikace;
            zakladatel = d.hodnotaUdaje.zakladatelSpecified ? d.hodnotaUdaje.zakladatel : null;
            prima_ucast = d.hodnotaUdaje.primaUcastSpecified ? d.hodnotaUdaje.primaUcast : null;
            valid = d.hodnotaUdaje.validSpecified ? d.hodnotaUdaje.valid : null;
            obmysleny = d.hodnotaUdaje.obmyslenySpecified ? d.hodnotaUdaje.obmysleny : null;
            spravce = d.hodnotaUdaje.spravceSpecified ? d.hodnotaUdaje.spravce : null;
            typ = d.hodnotaUdaje.typ;
            protektor = d.hodnotaUdaje.protektorSpecified ? d.hodnotaUdaje.protektor : null;
            postaveni = d.hodnotaUdaje.postaveni;
            postaveni_jinak = d.hodnotaUdaje.postaveniJinakSpecified ? d.hodnotaUdaje.postaveniJinak : null;
            rozdeleni_prostredku = d.hodnotaUdaje.rozdeleniProstredkuSpecified ? d.hodnotaUdaje.rozdeleniProstredku : null;
            rozdeleni_prostredku_podil = d.hodnotaUdaje.rozdeleniProstredkuPodil;
            spis_zn_sm = d.hodnotaUdaje.spisZnSM;
            urcen_pozici_ve_stat_org = d.hodnotaUdaje.urcenPoziciVeStatOrgSpecified ? d.hodnotaUdaje.urcenPoziciVeStatOrg : null;
            detail = d.hodnotaUdaje.detail;
            prima_ucast_podil = d.hodnotaUdaje.primaUcastPodil;
            slovni_vyjadreni = d.hodnotaUdaje.slovniVyjadreni;
            hlasovaci_pravo = d.hodnotaUdaje.hlasovaciPravo;
            disponuje = d.hodnotaUdaje.disponuje;
            podil = d.hodnotaUdaje.podil;
            email = d.hodnotaUdaje.email;
            osoba_jmeno = d.osoba.jmeno;
            osoba_prijmeni = d.osoba.prijmeni;
            osoba_titul_pred = d.osoba.titulPred;
            osoba_titul_za = d.osoba.titulZa;
            osoba_datum_narozeni = d.osoba.narozDatum;
            adresa_cast_obce = d.adresa?.castObce;
            adresa_cislo_ev = d.adresa?.cisloEv;
            adresa_cislo_or = d.adresa?.cisloOr;
            adresa_cislo_po = d.adresa?.cisloPo;
            adresa_obec = d.adresa?.obec;
            adresa_okres = d.adresa?.okres;
            adresa_psc = d.adresa?.psc;
            adresa_stat_nazev = d.adresa?.statNazev;
            adresa_text = d.adresa?.adresaText;
            bydliste_cast_obce = d.bydliste?.castObce;
            bydliste_cislo_or = d.bydliste?.cisloOr;
            bydliste_cislo_po = d.bydliste?.cisloPo;
            bydliste_obec = d.bydliste?.obec;
            bydliste_psc = d.bydliste?.psc;
            bydliste_stat_nazev = d.bydliste?.statNazev;
            bydliste_cislo_ev = d.bydliste?.cisloEv;
            bydliste_cislo_okres = d.bydliste?.okres;
            clenstvi_od = d.clenstviOdSpecified ? d.clenstviOd : null;
            clenstvi_do = d.clenstviDoSpecified ? d.clenstviDo : null;
            podil_na_prospechu_hodnota = d.hodnotaUdaje.podilNaProspechu?.textValue;
            podil_na_prospechu_typ = d.hodnotaUdaje.podilNaProspechu?.typ;
            podil_na_hlasovani_hodnota = d.hodnotaUdaje.podilNaHlasovani?.textValue;
            podil_na_hlasovani_typ = d.hodnotaUdaje.podilNaHlasovani?.typ;

            vlastni_podil_na_hlasovani = d.hodnotaUdaje.vlastniPodilNaHlasovaniSpecified ? d.hodnotaUdaje.vlastniPodilNaHlasovani : null;
            vlastni_podil_na_prospechu = d.hodnotaUdaje.vlastniPodilNaProspechuSpecified ? d.hodnotaUdaje.vlastniPodilNaProspechu : null;
            znepristupneni = d.hodnotaUdaje.znepristupneniSpecified ? d.hodnotaUdaje.znepristupneni : null;
            uverejneni = d.hodnotaUdaje.uverejneniSpecified ? d.hodnotaUdaje.uverejneni : null;
            smlouvaVliv = d.hodnotaUdaje.smlouvaVlivSpecified ? d.hodnotaUdaje.smlouvaVliv : null;

            if (d.hodnotaUdaje?.strukturaVztahu?.retezce?.linkedhashmap?.clanky != null)
            {
                struktura_vztahu_k_majiteli = d.hodnotaUdaje.strukturaVztahu.retezce.linkedhashmap.clanky
                    .Select(m => new struktura_vztahu_majitel()
                    {
                        typ = m.typClanku,
                        id = m.identifikace?.ico,
                        jmeno = m.identifikace?.name,
                        jedna_ve_shode = m.vztahKPredchozimuClanku?.jednaVeShode ?? false,
                        jedna_ve_shode_s_osoby = null, //TODO m.vztahKPredchozimuClanku?.jednaVeShodeSOsoby
                        podil_na_prospechu_hodnota = m.vztahKPredchozimuClanku?.podilNaProspechu?.textValue,
                        podil_na_prospechu_typ = m.vztahKPredchozimuClanku?.podilNaProspechu?.typ,
                        vlastni_podil_na_prospechu = m.vztahKPredchozimuClanku?.vlastniPodilNaProspechu ?? false
                    })
                    .ToArray();
            }
        }

        public DateTime datum_zapis { get; set; }
        public DateTime? datum_vymaz { get; set; }
        public string udaj_typ { get; set; } //PRIMY_SKUTECNY_MAJITEL / SKUTECNY_MAJITEL
        public string udaj_typ_nazev { get; set; } //PRIMY_SKUTECNY_MAJITEL / SKUTECNY_MAJITEL
        public string specifikace { get; set; }
        public bool? zakladatel { get; set; }
        public bool? prima_ucast { get; set; }
        public bool? valid { get; set; }
        public bool? obmysleny { get; set; }
        public bool? spravce { get; set; }
        public string typ { get; set; }
        public bool? protektor { get; set; }
        public bool? postaveni_jinak { get; set; }
        public string postaveni { get; set; }
        public bool? rozdeleni_prostredku { get; set; }
        public string spis_zn_sm { get; set; }
        public bool? urcen_pozici_ve_stat_org { get; set; }
        public string detail { get; set; }
        public string prima_ucast_podil { get; set; }
        public string slovni_vyjadreni { get; set; }
        public string hlasovaci_pravo { get; set; }
        public string disponuje { get; set; }
        public string rozdeleni_prostredku_podil { get; set; }
        public string email { get; set; }
        public string podil { get; set; }
        public string osobaId { get; set; }

        public string osoba_jmeno { get; set; }
        public string osoba_prijmeni { get; set; }
        public DateTime osoba_datum_narozeni { get; set; }
        public string osoba_titul_pred { get; set; }
        public string osoba_titul_za { get; set; }
        public string adresa_stat_nazev { get; set; }
        public string adresa_obec { get; set; }
        public string adresa_cast_obce { get; set; }
        public string adresa_ulice { get; set; }
        public string adresa_cislo_po { get; set; }
        public string adresa_cislo_or { get; set; }
        public string adresa_psc { get; set; }
        public string adresa_okres { get; set; }
        public string adresa_cislo_ev { get; set; }
        public string adresa_text { get; set; }
        public string bydliste_stat_nazev { get; set; }
        public string bydliste_obec { get; set; }
        public string bydliste_cast_obce { get; set; }
        public string bydliste_cislo_ev { get; set; }
        public string bydliste_cislo_okres { get; set; }

        public string bydliste_cislo_po { get; set; }
        public string bydliste_cislo_or { get; set; }
        public string bydliste_psc { get; set; }
        public string bydliste_ulice { get; set; }
        public DateTime? clenstvi_od { get; set; }
        public DateTime? clenstvi_do { get; set; }
        public string podil_na_prospechu_typ { get; set; }
        public string podil_na_prospechu_hodnota { get; set; }
        public string podil_na_hlasovani_typ { get; set; }
        public string podil_na_hlasovani_hodnota { get; set; }
        public bool? vlastni_podil_na_hlasovani { get; set; }
        public bool? vlastni_podil_na_prospechu { get; set; }
        public bool? znepristupneni { get; set; }
        public bool? uverejneni { get; set; }
        public string jina_skutecnost_prijemce { get; set; }

        public bool? pravo_veta { get; set; }
        public bool? clenVolenehoOrganu { get; set; }
        public bool? jinaSkutecnostPrijemce { get; set; }
        public bool? smlouvaVliv { get; set; }

        public struktura_vztahu_majitel[] struktura_vztahu_k_majiteli { get; set; }
        public class struktura_vztahu_majitel
        {
            public string typ { get; set; }
            public string jmeno { get; set; }
            public string id { get; set; }
            public bool vlastni_podil_na_prospechu { get; set; }
            public string podil_na_prospechu_typ { get; set; }
            public string podil_na_prospechu_hodnota { get; set; }
            public bool jedna_ve_shode { get; set; }
            public string jedna_ve_shode_s_osoby { get; set; }

        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(osoba_prijmeni, osoba_jmeno, datum_zapis, datum_vymaz);
        }
    }


}
