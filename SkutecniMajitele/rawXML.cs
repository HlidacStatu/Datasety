using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkutecniMajitele
{


    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false,ElementName ="xml")]
    public partial class rawXML
    {

        private xmlSubjekt[] subjektField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Subjekt")]
        public xmlSubjekt[] Subjekt
        {
            get
            {
                return this.subjektField;
            }
            set
            {
                this.subjektField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjekt
    {

        private string nazevField;

        private string icoField;

        private System.DateTime zapisDatumField;
        private System.DateTime vymazDatumField;

        private bool vymazDatumFieldSpecified;


        private xmlSubjektUdaj[] udajeField;

        /// <remarks/>
        public string nazev
        {
            get
            {
                return this.nazevField;
            }
            set
            {
                this.nazevField = value;
            }
        }

        /// <remarks/>
        public string ico
        {
            get
            {
                return this.icoField;
            }
            set
            {
                this.icoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime zapisDatum
        {
            get
            {
                return this.zapisDatumField;
            }
            set
            {
                this.zapisDatumField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime vymazDatum
        {
            get
            {
                return this.vymazDatumField;
            }
            set
            {
                this.vymazDatumField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool vymazDatumSpecified
        {
            get
            {
                return this.vymazDatumFieldSpecified;
            }
            set
            {
                this.vymazDatumFieldSpecified = value;
            }
        }


        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Udaj", IsNullable = false)]
        public xmlSubjektUdaj[] udaje
        {
            get
            {
                return this.udajeField;
            }
            set
            {
                this.udajeField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdaj
    {

        private string hlavickaField;

        private System.DateTime zapisDatumField;

        private System.DateTime vymazDatumField;

        private bool vymazDatumFieldSpecified;

        private xmlSubjektUdajHodnotaUdaje hodnotaUdajeField;

        private string hodnotaTextField;

        private xmlSubjektUdajUdajTyp udajTypField;

        private xmlSubjektUdajUdaj[] podudajeField;

        private xmlSubjektUdajPravniForma pravniFormaField;

        private xmlSubjektUdajAdresa adresaField;

        private xmlSubjektUdajSpisZn spisZnField;

        /// <remarks/>
        public string hlavicka
        {
            get
            {
                return this.hlavickaField;
            }
            set
            {
                this.hlavickaField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime zapisDatum
        {
            get
            {
                return this.zapisDatumField;
            }
            set
            {
                this.zapisDatumField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime vymazDatum
        {
            get
            {
                return this.vymazDatumField;
            }
            set
            {
                this.vymazDatumField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool vymazDatumSpecified
        {
            get
            {
                return this.vymazDatumFieldSpecified;
            }
            set
            {
                this.vymazDatumFieldSpecified = value;
            }
        }

        /// <remarks/>
        public xmlSubjektUdajHodnotaUdaje hodnotaUdaje
        {
            get
            {
                return this.hodnotaUdajeField;
            }
            set
            {
                this.hodnotaUdajeField = value;
            }
        }

        /// <remarks/>
        public string hodnotaText
        {
            get
            {
                return this.hodnotaTextField;
            }
            set
            {
                this.hodnotaTextField = value;
            }
        }

        /// <remarks/>
        public xmlSubjektUdajUdajTyp udajTyp
        {
            get
            {
                return this.udajTypField;
            }
            set
            {
                this.udajTypField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Udaj", IsNullable = false)]
        public xmlSubjektUdajUdaj[] podudaje
        {
            get
            {
                return this.podudajeField;
            }
            set
            {
                this.podudajeField = value;
            }
        }

        /// <remarks/>
        public xmlSubjektUdajPravniForma pravniForma
        {
            get
            {
                return this.pravniFormaField;
            }
            set
            {
                this.pravniFormaField = value;
            }
        }

        /// <remarks/>
        public xmlSubjektUdajAdresa adresa
        {
            get
            {
                return this.adresaField;
            }
            set
            {
                this.adresaField = value;
            }
        }

        /// <remarks/>
        public xmlSubjektUdajSpisZn spisZn
        {
            get
            {
                return this.spisZnField;
            }
            set
            {
                this.spisZnField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajHodnotaUdaje
    {

        private xmlSubjektUdajHodnotaUdajeVklad vkladField;
        private string textField;

        private xmlSubjektUdajHodnotaUdajeSplaceni splaceniField;

        /// <remarks/>
        public xmlSubjektUdajHodnotaUdajeVklad vklad
        {
            get
            {
                return this.vkladField;
            }
            set
            {
                this.vkladField = value;
            }
        }

        /// <remarks/>
        public string text
        {
            get
            {
                return this.textField;
            }
            set
            {
                this.textField = value;
            }
        }

        /// <remarks/>

        public xmlSubjektUdajHodnotaUdajeSplaceni splaceni
        {
            get
            {
                return this.splaceniField;
            }
            set
            {
                this.splaceniField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajHodnotaUdajeVklad
    {

        private string typField;

        private string textValueField;

        /// <remarks/>
        public string typ
        {
            get
            {
                return this.typField;
            }
            set
            {
                this.typField = value;
            }
        }

        /// <remarks/>
        public string textValue
        {
            get
            {
                return this.textValueField;
            }
            set
            {
                this.textValueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajHodnotaUdajeSplaceni
    {

        private string typField;

        private string textValueField;

        /// <remarks/>
        public string typ
        {
            get
            {
                return this.typField;
            }
            set
            {
                this.typField = value;
            }
        }

        /// <remarks/>
        public string textValue
        {
            get
            {
                return this.textValueField;
            }
            set
            {
                this.textValueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajUdajTyp
    {

        private string kodField;

        private string nazevField;

        /// <remarks/>
        public string kod
        {
            get
            {
                return this.kodField;
            }
            set
            {
                this.kodField = value;
            }
        }

        /// <remarks/>
        public string nazev
        {
            get
            {
                return this.nazevField;
            }
            set
            {
                this.nazevField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajUdaj
    {

        private string hlavickaField;

        private System.DateTime zapisDatumField;

        private System.DateTime vymazDatumField;

        private bool vymazDatumFieldSpecified;

        private string hodnotaTextField;

        private xmlSubjektUdajUdajHodnotaUdaje hodnotaUdajeField;

        private System.DateTime clenstviOdField;

        private bool clenstviOdFieldSpecified;

        private System.DateTime clenstviDoField;

        private bool clenstviDoFieldSpecified;

        private System.DateTime funkceOdField;

        private bool funkceOdFieldSpecified;

        private System.DateTime funkceDoField;

        private bool funkceDoFieldSpecified;

        private string funkceField;

        private xmlSubjektUdajUdajUdajTyp udajTypField;

        private xmlSubjektUdajUdajOsoba osobaField;

        private xmlSubjektUdajUdajAdresa adresaField;
        private xmlSubjektUdajUdajBydliste bydlisteField;
        //private xmlSubjektUdajUdajUdaj[] podudajeField;

        /// <remarks/>
        public string hlavicka
        {
            get
            {
                return this.hlavickaField;
            }
            set
            {
                this.hlavickaField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime zapisDatum
        {
            get
            {
                return this.zapisDatumField;
            }
            set
            {
                this.zapisDatumField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime vymazDatum
        {
            get
            {
                return this.vymazDatumField;
            }
            set
            {
                this.vymazDatumField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool vymazDatumSpecified
        {
            get
            {
                return this.vymazDatumFieldSpecified;
            }
            set
            {
                this.vymazDatumFieldSpecified = value;
            }
        }

        /// <remarks/>
        public string hodnotaText
        {
            get
            {
                return this.hodnotaTextField;
            }
            set
            {
                this.hodnotaTextField = value;
            }
        }

        /// <remarks/>
        public xmlSubjektUdajUdajHodnotaUdaje hodnotaUdaje
        {
            get
            {
                return this.hodnotaUdajeField;
            }
            set
            {
                this.hodnotaUdajeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime clenstviOd
        {
            get
            {
                return this.clenstviOdField;
            }
            set
            {
                this.clenstviOdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool clenstviOdSpecified
        {
            get
            {
                return this.clenstviOdFieldSpecified;
            }
            set
            {
                this.clenstviOdFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime clenstviDo
        {
            get
            {
                return this.clenstviDoField;
            }
            set
            {
                this.clenstviDoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool clenstviDoSpecified
        {
            get
            {
                return this.clenstviDoFieldSpecified;
            }
            set
            {
                this.clenstviDoFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime funkceOd
        {
            get
            {
                return this.funkceOdField;
            }
            set
            {
                this.funkceOdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool funkceOdSpecified
        {
            get
            {
                return this.funkceOdFieldSpecified;
            }
            set
            {
                this.funkceOdFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime funkceDo
        {
            get
            {
                return this.funkceDoField;
            }
            set
            {
                this.funkceDoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool funkceDoSpecified
        {
            get
            {
                return this.funkceDoFieldSpecified;
            }
            set
            {
                this.funkceDoFieldSpecified = value;
            }
        }

        /// <remarks/>
        public string funkce
        {
            get
            {
                return this.funkceField;
            }
            set
            {
                this.funkceField = value;
            }
        }

        /// <remarks/>
        public xmlSubjektUdajUdajUdajTyp udajTyp
        {
            get
            {
                return this.udajTypField;
            }
            set
            {
                this.udajTypField = value;
            }
        }

        /// <remarks/>
        public xmlSubjektUdajUdajOsoba osoba
        {
            get
            {
                return this.osobaField;
            }
            set
            {
                this.osobaField = value;
            }
        }

        /// <remarks/>
        public xmlSubjektUdajUdajAdresa adresa
        {
            get
            {
                return this.adresaField;
            }
            set
            {
                this.adresaField = value;
            }
        }

        ///// <remarks/>
        //[System.Xml.Serialization.XmlArrayItemAttribute("Udaj", IsNullable = false)]
        //public xmlSubjektUdajUdajUdaj[] podudaje
        //{
        //    get
        //    {
        //        return this.podudajeField;
        //    }
        //    set
        //    {
        //        this.podudajeField = value;
        //    }
        //}


        public xmlSubjektUdajUdajBydliste bydliste
        {
            get
            {
                return this.bydlisteField;
            }
            set
            {
                this.bydlisteField = value;
            }
        }

    }
    /// <remarks/>

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajUdajHodnotaUdajeStrukturaVztahu
    {

        private xmlSubjektUdajUdajHodnotaUdajeStrukturaVztahuRetezce retezceField;

        /// <remarks/>
        public xmlSubjektUdajUdajHodnotaUdajeStrukturaVztahuRetezce retezce
        {
            get
            {
                return this.retezceField;
            }
            set
            {
                this.retezceField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajUdajHodnotaUdajeStrukturaVztahuRetezce
    {

        private xmlSubjektUdajUdajHodnotaUdajeStrukturaVztahuRetezceLinkedhashmap linkedhashmapField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("linked-hash-map")]
        public xmlSubjektUdajUdajHodnotaUdajeStrukturaVztahuRetezceLinkedhashmap linkedhashmap
        {
            get
            {
                return this.linkedhashmapField;
            }
            set
            {
                this.linkedhashmapField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajUdajHodnotaUdajeStrukturaVztahuRetezceLinkedhashmap
    {

        private xmlSubjektUdajUdajHodnotaUdajeStrukturaVztahuRetezceLinkedhashmapLinkedhashmap[] clankyField;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("linked-hash-map", IsNullable = false)]
        public xmlSubjektUdajUdajHodnotaUdajeStrukturaVztahuRetezceLinkedhashmapLinkedhashmap[] clanky
        {
            get
            {
                return this.clankyField;
            }
            set
            {
                this.clankyField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajUdajHodnotaUdajeStrukturaVztahuRetezceLinkedhashmapLinkedhashmap
    {

        private string typClankuField;

        private xmlSubjektUdajUdajHodnotaUdajeStrukturaVztahuRetezceLinkedhashmapLinkedhashmapIdentifikace identifikaceField;

        private xmlSubjektUdajUdajHodnotaUdajeStrukturaVztahuRetezceLinkedhashmapLinkedhashmapVztahKPredchozimuClanku vztahKPredchozimuClankuField;

        /// <remarks/>
        public string typClanku
        {
            get
            {
                return this.typClankuField;
            }
            set
            {
                this.typClankuField = value;
            }
        }

        /// <remarks/>
        public xmlSubjektUdajUdajHodnotaUdajeStrukturaVztahuRetezceLinkedhashmapLinkedhashmapIdentifikace identifikace
        {
            get
            {
                return this.identifikaceField;
            }
            set
            {
                this.identifikaceField = value;
            }
        }

        /// <remarks/>
        public xmlSubjektUdajUdajHodnotaUdajeStrukturaVztahuRetezceLinkedhashmapLinkedhashmapVztahKPredchozimuClanku vztahKPredchozimuClanku
        {
            get
            {
                return this.vztahKPredchozimuClankuField;
            }
            set
            {
                this.vztahKPredchozimuClankuField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajUdajHodnotaUdajeStrukturaVztahuRetezceLinkedhashmapLinkedhashmapIdentifikace
    {

        private string nameField;

        private string icoField;

        /// <remarks/>
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        public string ico
        {
            get
            {
                return this.icoField;
            }
            set
            {
                this.icoField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajUdajHodnotaUdajeStrukturaVztahuRetezceLinkedhashmapLinkedhashmapVztahKPredchozimuClanku
    {

        private bool vlastniPodilNaProspechuField;

        private xmlSubjektUdajUdajHodnotaUdajeStrukturaVztahuRetezceLinkedhashmapLinkedhashmapVztahKPredchozimuClankuPodilNaProspechu podilNaProspechuField;

        private bool jednaVeShodeField;

        private object jednaVeShodeSOsobyField;

        private bool jinyVztahField;

        private bool vrcholoveVedeniField;

        private bool vrcholoveVedeniFieldSpecified;

        /// <remarks/>
        public bool vlastniPodilNaProspechu
        {
            get
            {
                return this.vlastniPodilNaProspechuField;
            }
            set
            {
                this.vlastniPodilNaProspechuField = value;
            }
        }

        /// <remarks/>
        public xmlSubjektUdajUdajHodnotaUdajeStrukturaVztahuRetezceLinkedhashmapLinkedhashmapVztahKPredchozimuClankuPodilNaProspechu podilNaProspechu
        {
            get
            {
                return this.podilNaProspechuField;
            }
            set
            {
                this.podilNaProspechuField = value;
            }
        }

        /// <remarks/>
        public bool jednaVeShode
        {
            get
            {
                return this.jednaVeShodeField;
            }
            set
            {
                this.jednaVeShodeField = value;
            }
        }

        /// <remarks/>
        public object jednaVeShodeSOsoby
        {
            get
            {
                return this.jednaVeShodeSOsobyField;
            }
            set
            {
                this.jednaVeShodeSOsobyField = value;
            }
        }

        /// <remarks/>
        public bool jinyVztah
        {
            get
            {
                return this.jinyVztahField;
            }
            set
            {
                this.jinyVztahField = value;
            }
        }

        /// <remarks/>
        public bool vrcholoveVedeni
        {
            get
            {
                return this.vrcholoveVedeniField;
            }
            set
            {
                this.vrcholoveVedeniField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool vrcholoveVedeniSpecified
        {
            get
            {
                return this.vrcholoveVedeniFieldSpecified;
            }
            set
            {
                this.vrcholoveVedeniFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajUdajHodnotaUdajeStrukturaVztahuRetezceLinkedhashmapLinkedhashmapVztahKPredchozimuClankuPodilNaProspechu
    {

        private string typField;

        private string textValueField;

        /// <remarks/>
        public string typ
        {
            get
            {
                return this.typField;
            }
            set
            {
                this.typField = value;
            }
        }

        /// <remarks/>
        public string textValue
        {
            get
            {
                return this.textValueField;
            }
            set
            {
                this.textValueField = value;
            }
        }
    }



    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajUdajHodnotaUdaje
    {

        private string specifikaceField;

        private bool validField;

        private bool validFieldSpecified;

        private bool obmyslenyField;

        private bool obmyslenyFieldSpecified;

        private bool spravceField;

        private bool spravceFieldSpecified;

        private string podobaField;

        private string typField;

        private string textField;

        private string pocetField;

        private bool pocetFieldSpecified;

        private xmlSubjektUdajUdajHodnotaUdajeHodnota hodnotaField;

        private string pocetClenuField;

        private bool pocetClenuFieldSpecified;

        private string tField;

        private bool autoprupisField;
        private string postaveniField;

        private xmlSubjektUdajUdajHodnotaUdajePodilNaProspechu podilNaProspechuField;

        private object nahradniSMOsobyField;
        
        private xmlSubjektUdajUdajHodnotaUdajeStrukturaVztahu strukturaVztahuField;

        private xmlSubjektUdajUdajHodnotaUdajePodilNaHlasovani podilNaHlasovaniField;

        private object jednaVeShodeSOsobyField;

        private bool vlastniPodilNaHlasovaniField;

        private bool vlastniPodilNaHlasovaniFieldSpecified;

        private bool vlastniPodilNaProspechuField;

        private bool vlastniPodilNaProspechuFieldSpecified;

        private bool zakladatelField;

        private bool zakladatelFieldSpecified;

        private bool protektorField;

        private bool protektorFieldSpecified;

        private bool postaveniJinakField;

        private bool postaveniJinakFieldSpecified;

        private bool primaUcastField;

        private bool primaUcastFieldSpecified;

        private bool rozdeleniProstredkuField;
        private string rozdeleniProstredkuPodilField;

        private bool rozdeleniProstredkuFieldSpecified;

        private string spisZnSMField;

        private bool urcenPoziciVeStatOrgField;

        private bool urcenPoziciVeStatOrgFieldSpecified;

        private string primaUcastPodilField;
        private string detailField;
        private string slovniVyjadreniField;
        private string hlasovaciPravoField;
        private string disponujeField;
        private string podilField;
        private string emailField;


        private bool pravoVetaField;
        private bool pravoVetaFieldSpecified;

        private bool znepristupneniField;
        private bool znepristupneniFieldSpecified;

        private bool clenVolenehoOrganuField;
        private bool clenVolenehoOrganuFieldSpecified;

        private bool jinaSkutecnostPrijemceField;
        private bool jinaSkutecnostPrijemceFieldSpecified;

        private bool smlouvaVlivField;
        private bool smlouvaVlivFieldSpecified;

        private bool uverejneniField;
        private bool uverejneniFieldSpecified;

        private string koncovyPrijemceTextField;


        /// <remarks/>
        public bool pravoVeta
        {
            get { return this.pravoVetaField; }
            set { this.pravoVetaField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool pravoVetaSpecified
        {
            get { return this.pravoVetaFieldSpecified; }
            set { this.pravoVetaFieldSpecified = value; }
        }

        /// <remarks/>
        public bool znepristupneni
        {
            get { return this.znepristupneniField; }
            set { this.znepristupneniField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool znepristupneniSpecified
        {
            get { return this.znepristupneniFieldSpecified; }
            set { this.znepristupneniFieldSpecified = value; }
        }

        /// <remarks/>
        public bool clenVolenehoOrganu
        {
            get { return this.clenVolenehoOrganuField; }
            set { this.clenVolenehoOrganuField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool clenVolenehoOrganuSpecified
        {
            get { return this.clenVolenehoOrganuFieldSpecified; }
            set { this.clenVolenehoOrganuFieldSpecified = value; }
        }

        /// <remarks/>
        public bool jinaSkutecnostPrijemce
        {
            get { return this.jinaSkutecnostPrijemceField; }
            set { this.jinaSkutecnostPrijemceField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool jinaSkutecnostPrijemceSpecified
        {
            get { return this.jinaSkutecnostPrijemceFieldSpecified; }
            set { this.jinaSkutecnostPrijemceFieldSpecified = value; }
        }

        /// <remarks/>
        public bool uverejneni
        {
            get { return this.uverejneniField; }
            set { this.uverejneniField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool uverejneniSpecified
        {
            get { return this.uverejneniFieldSpecified; }
            set { this.uverejneniFieldSpecified = value; }
        }



        /// <remarks/>
        public bool smlouvaVliv
        {
            get { return this.smlouvaVlivField; }
            set { this.smlouvaVlivField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool smlouvaVlivSpecified
        {
            get { return this.smlouvaVlivFieldSpecified; }
            set { this.smlouvaVlivFieldSpecified = value; }
        }


        /// <remarks/>
        public string koncovyPrijemceText
        {
            get
            {
                return this.koncovyPrijemceTextField;
            }
            set
            {
                this.koncovyPrijemceTextField = value;
            }
        }
        /// <remarks/>
        public bool valid
        {
            get { return this.validField; }
            set { this.validField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool validSpecified
        {
            get { return this.validFieldSpecified; }
            set { this.validFieldSpecified = value; }
        }

        /// <remarks/>
        public bool obmysleny
        {
            get
            {
                return this.obmyslenyField;
            }
            set
            {
                this.obmyslenyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool obmyslenySpecified
        {
            get
            {
                return this.obmyslenyFieldSpecified;
            }
            set
            {
                this.obmyslenyFieldSpecified = value;
            }
        }

        /// <remarks/>
        public bool spravce
        {
            get
            {
                return this.spravceField;
            }
            set
            {
                this.spravceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool spravceSpecified
        {
            get
            {
                return this.spravceFieldSpecified;
            }
            set
            {
                this.spravceFieldSpecified = value;
            }
        }

        /// <remarks/>
        public string podoba
        {
            get
            {
                return this.podobaField;
            }
            set
            {
                this.podobaField = value;
            }
        }
        public string specifikace
        {
            get
            {
                return this.specifikaceField;
            }
            set
            {
                this.specifikaceField = value;
            }
        }

        /// <remarks/>
        public string typ
        {
            get
            {
                return this.typField;
            }
            set
            {
                this.typField = value;
            }
        }

        /// <remarks/>
        public string text
        {
            get
            {
                return this.textField;
            }
            set
            {
                this.textField = value;
            }
        }

        /// <remarks/>
        public string pocet
        {
            get
            {
                return this.pocetField;
            }
            set
            {
                this.pocetField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool pocetSpecified
        {
            get
            {
                return this.pocetFieldSpecified;
            }
            set
            {
                this.pocetFieldSpecified = value;
            }
        }

        /// <remarks/>
        public xmlSubjektUdajUdajHodnotaUdajeHodnota hodnota
        {
            get
            {
                return this.hodnotaField;
            }
            set
            {
                this.hodnotaField = value;
            }
        }

        /// <remarks/>
        public string pocetClenu
        {
            get
            {
                return this.pocetClenuField;
            }
            set
            {
                this.pocetClenuField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool pocetClenuSpecified
        {
            get
            {
                return this.pocetClenuFieldSpecified;
            }
            set
            {
                this.pocetClenuFieldSpecified = value;
            }
        }

        /// <remarks/>
        public string T
        {
            get
            {
                return this.tField;
            }
            set
            {
                this.tField = value;
            }
        }

        /// <remarks/>
        public bool autoprupis
        {
            get
            {
                return this.autoprupisField;
            }
            set
            {
                this.autoprupisField = value;
            }
        }

        /// <remarks/>
        public string postaveni
        {
            get
            {
                return this.postaveniField;
            }
            set
            {
                this.postaveniField = value;
            }
        }

        /// <remarks/>
        public xmlSubjektUdajUdajHodnotaUdajePodilNaProspechu podilNaProspechu
        {
            get
            {
                return this.podilNaProspechuField;
            }
            set
            {
                this.podilNaProspechuField = value;
            }
        }

        /// <remarks/>
        public object nahradniSMOsoby
        {
            get
            {
                return this.nahradniSMOsobyField;
            }
            set
            {
                this.nahradniSMOsobyField = value;
            }
        }

        /// <remarks/>
        public xmlSubjektUdajUdajHodnotaUdajePodilNaHlasovani podilNaHlasovani
        {
            get
            {
                return this.podilNaHlasovaniField;
            }
            set
            {
                this.podilNaHlasovaniField = value;
            }
        }

        /// <remarks/>
        public object jednaVeShodeSOsoby
        {
            get
            {
                return this.jednaVeShodeSOsobyField;
            }
            set
            {
                this.jednaVeShodeSOsobyField = value;
            }
        }

        public xmlSubjektUdajUdajHodnotaUdajeStrukturaVztahu strukturaVztahu
        {
            get { return this.strukturaVztahuField; }
            set { this.strukturaVztahuField = value; }
        }

        /// <remarks/>
        public bool vlastniPodilNaHlasovani
        {
            get
            {
                return this.vlastniPodilNaHlasovaniField;
            }
            set
            {
                this.vlastniPodilNaHlasovaniField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool vlastniPodilNaHlasovaniSpecified
        {
            get
            {
                return this.vlastniPodilNaHlasovaniFieldSpecified;
            }
            set
            {
                this.vlastniPodilNaHlasovaniFieldSpecified = value;
            }
        }

        /// <remarks/>
        public bool vlastniPodilNaProspechu
        {
            get
            {
                return this.vlastniPodilNaProspechuField;
            }
            set
            {
                this.vlastniPodilNaProspechuField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool vlastniPodilNaProspechuSpecified
        {
            get
            {
                return this.vlastniPodilNaProspechuFieldSpecified;
            }
            set
            {
                this.vlastniPodilNaProspechuFieldSpecified = value;
            }
        }

        /// <remarks/>
        public bool zakladatel
        {
            get
            {
                return this.zakladatelField;
            }
            set
            {
                this.zakladatelField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool zakladatelSpecified
        {
            get
            {
                return this.zakladatelFieldSpecified;
            }
            set
            {
                this.zakladatelFieldSpecified = value;
            }
        }

        /// <remarks/>
        public bool protektor
        {
            get
            {
                return this.protektorField;
            }
            set
            {
                this.protektorField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool protektorSpecified
        {
            get
            {
                return this.protektorFieldSpecified;
            }
            set
            {
                this.protektorFieldSpecified = value;
            }
        }

        /// <remarks/>
        public bool postaveniJinak
        {
            get
            {
                return this.postaveniJinakField;
            }
            set
            {
                this.postaveniJinakField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool postaveniJinakSpecified
        {
            get
            {
                return this.postaveniJinakFieldSpecified;
            }
            set
            {
                this.postaveniJinakFieldSpecified = value;
            }
        }

        /// <remarks/>
        public bool primaUcast
        {
            get
            {
                return this.primaUcastField;
            }
            set
            {
                this.primaUcastField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool primaUcastSpecified
        {
            get
            {
                return this.primaUcastFieldSpecified;
            }
            set
            {
                this.primaUcastFieldSpecified = value;
            }
        }

        /// <remarks/>
        public bool rozdeleniProstredku
        {
            get
            {
                return this.rozdeleniProstredkuField;
            }
            set
            {
                this.rozdeleniProstredkuField = value;
            }
        }

        public string rozdeleniProstredkuPodil
        {
            get
            {
                return this.rozdeleniProstredkuPodilField;
            }
            set
            {
                this.rozdeleniProstredkuPodilField= value;
            }
        }
        public string detail
        {
            get
            {
                return this.detailField;
            }
            set
            {
                this.detailField = value;
            }
        }

        public string hlasovaciPravo
        {
            get
            {
                return this.hlasovaciPravoField;
            }
            set
            {
                this.hlasovaciPravoField = value;
            }
        }
        public string slovniVyjadreni
        {
            get
            {
                return this.slovniVyjadreniField;
            }
            set
            {
                this.slovniVyjadreniField = value;
            }
        }

        public string disponuje
        {
            get
            {
                return this.disponujeField;
            }
            set
            {
                this.disponujeField = value;
            }
        }
        public string podil
        {
            get
            {
                return this.podilField;
            }
            set
            {
                this.podilField = value;
            }
        }

        public string email
        {
            get
            {
                return this.emailField;
            }
            set
            {
                this.emailField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool rozdeleniProstredkuSpecified
        {
            get
            {
                return this.rozdeleniProstredkuFieldSpecified;
            }
            set
            {
                this.rozdeleniProstredkuFieldSpecified = value;
            }
        }

        /// <remarks/>
        public string spisZnSM
        {
            get
            {
                return this.spisZnSMField;
            }
            set
            {
                this.spisZnSMField = value;
            }
        }

        /// <remarks/>
        public bool urcenPoziciVeStatOrg
        {
            get
            {
                return this.urcenPoziciVeStatOrgField;
            }
            set
            {
                this.urcenPoziciVeStatOrgField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool urcenPoziciVeStatOrgSpecified
        {
            get
            {
                return this.urcenPoziciVeStatOrgFieldSpecified;
            }
            set
            {
                this.urcenPoziciVeStatOrgFieldSpecified = value;
            }
        }

        /// <remarks/>
        public string primaUcastPodil
        {
            get
            {
                return this.primaUcastPodilField;
            }
            set
            {
                this.primaUcastPodilField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajUdajHodnotaUdajeHodnota
    {

        private string typField;

        private string textValueField;

        /// <remarks/>
        public string typ
        {
            get
            {
                return this.typField;
            }
            set
            {
                this.typField = value;
            }
        }

        /// <remarks/>
        public string textValue
        {
            get
            {
                return this.textValueField;
            }
            set
            {
                this.textValueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajUdajHodnotaUdajePodilNaProspechu
    {

        private string typField;

        private string textValueField;

        /// <remarks/>
        public string typ
        {
            get
            {
                return this.typField;
            }
            set
            {
                this.typField = value;
            }
        }

        /// <remarks/>
        public string textValue
        {
            get
            {
                return this.textValueField;
            }
            set
            {
                this.textValueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajUdajHodnotaUdajePodilNaHlasovani
    {

        private string typField;

        private string textValueField;

        /// <remarks/>
        public string typ
        {
            get
            {
                return this.typField;
            }
            set
            {
                this.typField = value;
            }
        }

        /// <remarks/>
        public string textValue
        {
            get
            {
                return this.textValueField;
            }
            set
            {
                this.textValueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajUdajUdajTyp
    {

        private string kodField;

        private string nazevField;

        /// <remarks/>
        public string kod
        {
            get
            {
                return this.kodField;
            }
            set
            {
                this.kodField = value;
            }
        }

        /// <remarks/>
        public string nazev
        {
            get
            {
                return this.nazevField;
            }
            set
            {
                this.nazevField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajUdajOsoba
    {

        private string jmenoField;

        private string prijmeniField;

        private System.DateTime narozDatumField;

        private string titulPredField;

        private string titulZaField;

        /// <remarks/>
        public string jmeno
        {
            get
            {
                return this.jmenoField;
            }
            set
            {
                this.jmenoField = value;
            }
        }

        /// <remarks/>
        public string prijmeni
        {
            get
            {
                return this.prijmeniField;
            }
            set
            {
                this.prijmeniField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime narozDatum
        {
            get
            {
                return this.narozDatumField;
            }
            set
            {
                this.narozDatumField = value;
            }
        }

        /// <remarks/>
        public string titulPred
        {
            get
            {
                return this.titulPredField;
            }
            set
            {
                this.titulPredField = value;
            }
        }

        /// <remarks/>
        public string titulZa
        {
            get
            {
                return this.titulZaField;
            }
            set
            {
                this.titulZaField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajUdajAdresa
    {

        private string statNazevField;

        private string obecField;

        private string castObceField;

        private string uliceField;

        private string cisloPoField;

        private string cisloOrField;

        private string pscField;

        private string okresField;
        private string cisloEvField;
        private string adresaTextField;

        /// <remarks/>
        public string statNazev
        {
            get
            {
                return this.statNazevField;
            }
            set
            {
                this.statNazevField = value;
            }
        }

        /// <remarks/>
        public string obec
        {
            get
            {
                return this.obecField;
            }
            set
            {
                this.obecField = value;
            }
        }

        /// <remarks/>
        public string castObce
        {
            get
            {
                return this.castObceField;
            }
            set
            {
                this.castObceField = value;
            }
        }
        /// <remarks/>
        public string cisloEv
        {
            get
            {
                return this.cisloEvField;
            }
            set
            {
                this.cisloEvField = value;
            }
        }
        public string adresaText
        {
            get
            {
                return this.adresaTextField;
            }
            set
            {
                this.adresaTextField = value;
            }
        }

        /// <remarks/>
        public string ulice
        {
            get
            {
                return this.uliceField;
            }
            set
            {
                this.uliceField = value;
            }
        }

        /// <remarks/>
        public string cisloPo
        {
            get
            {
                return this.cisloPoField;
            }
            set
            {
                this.cisloPoField = value;
            }
        }

        /// <remarks/>
        public string cisloOr
        {
            get
            {
                return this.cisloOrField;
            }
            set
            {
                this.cisloOrField = value;
            }
        }

        /// <remarks/>
        public string psc
        {
            get
            {
                return this.pscField;
            }
            set
            {
                this.pscField = value;
            }
        }

        /// <remarks/>
        public string okres
        {
            get
            {
                return this.okresField;
            }
            set
            {
                this.okresField = value;
            }
        }
    }



    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajUdajBydliste
    {

        private string statNazevField;

        private string obecField;

        private string castObceField;

        private string uliceField;

        private string cisloPoField;

        private string cisloOrField;

        private string pscField;

        private string okresField;
        private string cisloEvField;
        private string adresaTextField;

        /// <remarks/>
        public string statNazev
        {
            get
            {
                return this.statNazevField;
            }
            set
            {
                this.statNazevField = value;
            }
        }

        /// <remarks/>
        public string obec
        {
            get
            {
                return this.obecField;
            }
            set
            {
                this.obecField = value;
            }
        }

        /// <remarks/>
        public string castObce
        {
            get
            {
                return this.castObceField;
            }
            set
            {
                this.castObceField = value;
            }
        }
        /// <remarks/>
        public string cisloEv
        {
            get
            {
                return this.cisloEvField;
            }
            set
            {
                this.cisloEvField = value;
            }
        }
        public string adresaText
        {
            get
            {
                return this.adresaTextField;
            }
            set
            {
                this.adresaTextField = value;
            }
        }

        /// <remarks/>
        public string ulice
        {
            get
            {
                return this.uliceField;
            }
            set
            {
                this.uliceField = value;
            }
        }

        /// <remarks/>
        public string cisloPo
        {
            get
            {
                return this.cisloPoField;
            }
            set
            {
                this.cisloPoField = value;
            }
        }

        /// <remarks/>
        public string cisloOr
        {
            get
            {
                return this.cisloOrField;
            }
            set
            {
                this.cisloOrField = value;
            }
        }

        /// <remarks/>
        public string psc
        {
            get
            {
                return this.pscField;
            }
            set
            {
                this.pscField = value;
            }
        }

        /// <remarks/>
        public string okres
        {
            get
            {
                return this.okresField;
            }
            set
            {
                this.okresField = value;
            }
        }
    }


    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajPravniForma
    {

        private string kodField;

        private string nazevField;

        private string zkratkaField;

        /// <remarks/>
        public string kod
        {
            get
            {
                return this.kodField;
            }
            set
            {
                this.kodField = value;
            }
        }

        /// <remarks/>
        public string nazev
        {
            get
            {
                return this.nazevField;
            }
            set
            {
                this.nazevField = value;
            }
        }

        /// <remarks/>
        public string zkratka
        {
            get
            {
                return this.zkratkaField;
            }
            set
            {
                this.zkratkaField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajAdresa
    {

        private string statNazevField;

        private string obecField;

        private string castObceField;

        private string uliceField;

        private string cisloPoField;

        private string cisloOrField;

        private string pscField;

        private string okresField;

        /// <remarks/>
        public string statNazev
        {
            get
            {
                return this.statNazevField;
            }
            set
            {
                this.statNazevField = value;
            }
        }

        /// <remarks/>
        public string obec
        {
            get
            {
                return this.obecField;
            }
            set
            {
                this.obecField = value;
            }
        }

        /// <remarks/>
        public string castObce
        {
            get
            {
                return this.castObceField;
            }
            set
            {
                this.castObceField = value;
            }
        }

        /// <remarks/>
        public string ulice
        {
            get
            {
                return this.uliceField;
            }
            set
            {
                this.uliceField = value;
            }
        }

        /// <remarks/>
        public string cisloPo
        {
            get
            {
                return this.cisloPoField;
            }
            set
            {
                this.cisloPoField = value;
            }
        }

        /// <remarks/>
        public string cisloOr
        {
            get
            {
                return this.cisloOrField;
            }
            set
            {
                this.cisloOrField = value;
            }
        }

        /// <remarks/>
        public string psc
        {
            get
            {
                return this.pscField;
            }
            set
            {
                this.pscField = value;
            }
        }

        /// <remarks/>
        public string okres
        {
            get
            {
                return this.okresField;
            }
            set
            {
                this.okresField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajSpisZn
    {

        private xmlSubjektUdajSpisZnSoud soudField;

        private string oddilField;

        private string vlozkaField;

        /// <remarks/>
        public xmlSubjektUdajSpisZnSoud soud
        {
            get
            {
                return this.soudField;
            }
            set
            {
                this.soudField = value;
            }
        }

        /// <remarks/>
        public string oddil
        {
            get
            {
                return this.oddilField;
            }
            set
            {
                this.oddilField = value;
            }
        }

        /// <remarks/>
        public string vlozka
        {
            get
            {
                return this.vlozkaField;
            }
            set
            {
                this.vlozkaField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajSpisZnSoud
    {

        private string kodField;

        private string nazevField;

        /// <remarks/>
        public string kod
        {
            get
            {
                return this.kodField;
            }
            set
            {
                this.kodField = value;
            }
        }

        /// <remarks/>
        public string nazev
        {
            get
            {
                return this.nazevField;
            }
            set
            {
                this.nazevField = value;
            }
        }
    }


}
