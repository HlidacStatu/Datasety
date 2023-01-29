using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkutecniMajitele.full
{

    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class xml
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

        private object[] itemsField;

        private ItemsChoiceType2[] itemsElementNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("adresa", typeof(xmlSubjektUdajAdresa))]
        [System.Xml.Serialization.XmlElementAttribute("hlavicka", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("hodnotaText", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("hodnotaUdaje", typeof(xmlSubjektUdajHodnotaUdaje))]
        [System.Xml.Serialization.XmlElementAttribute("podudaje", typeof(xmlSubjektUdajPodudaje))]
        [System.Xml.Serialization.XmlElementAttribute("pravniForma", typeof(xmlSubjektUdajPravniForma))]
        [System.Xml.Serialization.XmlElementAttribute("spisZn", typeof(xmlSubjektUdajSpisZn))]
        [System.Xml.Serialization.XmlElementAttribute("udajTyp", typeof(xmlSubjektUdajUdajTyp))]
        [System.Xml.Serialization.XmlElementAttribute("vymazDatum", typeof(System.DateTime), DataType = "date")]
        [System.Xml.Serialization.XmlElementAttribute("zapisDatum", typeof(System.DateTime), DataType = "date")]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("ItemsElementName")]
        public object[] Items
        {
            get
            {
                return this.itemsField;
            }
            set
            {
                this.itemsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ItemsElementName")]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public ItemsChoiceType2[] ItemsElementName
        {
            get
            {
                return this.itemsElementNameField;
            }
            set
            {
                this.itemsElementNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajAdresa
    {

        private object[] itemsField;

        private ItemsChoiceType1[] itemsElementNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("castObce", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("cisloEv", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("cisloOr", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("cisloPo", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("cisloText", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("doplnujiciText", typeof(object))]
        [System.Xml.Serialization.XmlElementAttribute("obec", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("okres", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("psc", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("statNazev", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("ulice", typeof(string))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("ItemsElementName")]
        public object[] Items
        {
            get
            {
                return this.itemsField;
            }
            set
            {
                this.itemsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ItemsElementName")]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public ItemsChoiceType1[] ItemsElementName
        {
            get
            {
                return this.itemsElementNameField;
            }
            set
            {
                this.itemsElementNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    public enum ItemsChoiceType1
    {

        /// <remarks/>
        castObce,

        /// <remarks/>
        cisloEv,

        /// <remarks/>
        cisloOr,

        /// <remarks/>
        cisloPo,

        /// <remarks/>
        cisloText,

        /// <remarks/>
        doplnujiciText,

        /// <remarks/>
        obec,

        /// <remarks/>
        okres,

        /// <remarks/>
        psc,

        /// <remarks/>
        statNazev,

        /// <remarks/>
        ulice,
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
    public partial class xmlSubjektUdajPodudaje
    {

        private xmlSubjektUdajPodudajeUdaj[] udajField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Udaj")]
        public xmlSubjektUdajPodudajeUdaj[] Udaj
        {
            get
            {
                return this.udajField;
            }
            set
            {
                this.udajField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajPodudajeUdaj
    {

        private string hlavickaField;

        private System.DateTime zapisDatumField;

        private System.DateTime vymazDatumField;

        private bool vymazDatumFieldSpecified;

        private string hodnotaTextField;

        private xmlSubjektUdajPodudajeUdajHodnotaUdaje hodnotaUdajeField;

        private System.DateTime clenstviOdField;

        private bool clenstviOdFieldSpecified;

        private System.DateTime clenstviDoField;

        private bool clenstviDoFieldSpecified;

        private System.DateTime funkceOdField;

        private bool funkceOdFieldSpecified;

        private System.DateTime funkceDoField;

        private bool funkceDoFieldSpecified;

        private string funkceField;

        private xmlSubjektUdajPodudajeUdajUdajTyp udajTypField;

        private xmlSubjektUdajPodudajeUdajOsoba osobaField;

        private xmlSubjektUdajPodudajeUdajAdresa adresaField;

        private xmlSubjektUdajPodudajeUdajUdaj[] podudajeField;

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
        public xmlSubjektUdajPodudajeUdajHodnotaUdaje hodnotaUdaje
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
        public xmlSubjektUdajPodudajeUdajUdajTyp udajTyp
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
        public xmlSubjektUdajPodudajeUdajOsoba osoba
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
        public xmlSubjektUdajPodudajeUdajAdresa adresa
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
        [System.Xml.Serialization.XmlArrayItemAttribute("Udaj", IsNullable = false)]
        public xmlSubjektUdajPodudajeUdajUdaj[] podudaje
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
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajPodudajeUdajHodnotaUdaje
    {

        private object[] itemsField;

        private ItemsChoiceType[] itemsElementNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("T", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("autoprupis", typeof(bool))]
        [System.Xml.Serialization.XmlElementAttribute("hodnota", typeof(xmlSubjektUdajPodudajeUdajHodnotaUdajeHodnota))]
        [System.Xml.Serialization.XmlElementAttribute("jednaVeShodeSOsoby", typeof(object))]
        [System.Xml.Serialization.XmlElementAttribute("koncovyPrijemceText", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("nahradniSMOsoby", typeof(object))]
        [System.Xml.Serialization.XmlElementAttribute("pocet", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("pocetClenu", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("podilNaHlasovani", typeof(xmlSubjektUdajPodudajeUdajHodnotaUdajePodilNaHlasovani))]
        [System.Xml.Serialization.XmlElementAttribute("podilNaProspechu", typeof(xmlSubjektUdajPodudajeUdajHodnotaUdajePodilNaProspechu))]
        [System.Xml.Serialization.XmlElementAttribute("podoba", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("postaveni", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("skutecnymMajitelemOd", typeof(System.DateTime), DataType = "date")]
        [System.Xml.Serialization.XmlElementAttribute("spoluvlastnictvi", typeof(bool))]
        [System.Xml.Serialization.XmlElementAttribute("spravce", typeof(bool))]
        [System.Xml.Serialization.XmlElementAttribute("strukturaVztahu", typeof(xmlSubjektUdajPodudajeUdajHodnotaUdajeStrukturaVztahu))]
        [System.Xml.Serialization.XmlElementAttribute("text", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("textZaOsobu", typeof(xmlSubjektUdajPodudajeUdajHodnotaUdajeTextZaOsobu))]
        [System.Xml.Serialization.XmlElementAttribute("textZruseni", typeof(object))]
        [System.Xml.Serialization.XmlElementAttribute("typ", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("vlastniPodilNaHlasovani", typeof(bool))]
        [System.Xml.Serialization.XmlElementAttribute("vlastniPodilNaProspechu", typeof(bool))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("ItemsElementName")]
        public object[] Items
        {
            get
            {
                return this.itemsField;
            }
            set
            {
                this.itemsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ItemsElementName")]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public ItemsChoiceType[] ItemsElementName
        {
            get
            {
                return this.itemsElementNameField;
            }
            set
            {
                this.itemsElementNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajPodudajeUdajHodnotaUdajeHodnota
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
    public partial class xmlSubjektUdajPodudajeUdajHodnotaUdajePodilNaHlasovani
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
    public partial class xmlSubjektUdajPodudajeUdajHodnotaUdajePodilNaProspechu
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
    public partial class xmlSubjektUdajPodudajeUdajHodnotaUdajeStrukturaVztahu
    {

        private object retezceField;

        /// <remarks/>
        public object retezce
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
    public partial class xmlSubjektUdajPodudajeUdajHodnotaUdajeTextZaOsobu
    {

        private string valueField;

        /// <remarks/>
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    public enum ItemsChoiceType
    {

        /// <remarks/>
        T,

        /// <remarks/>
        autoprupis,

        /// <remarks/>
        hodnota,

        /// <remarks/>
        jednaVeShodeSOsoby,

        /// <remarks/>
        koncovyPrijemceText,

        /// <remarks/>
        nahradniSMOsoby,

        /// <remarks/>
        pocet,

        /// <remarks/>
        pocetClenu,

        /// <remarks/>
        podilNaHlasovani,

        /// <remarks/>
        podilNaProspechu,

        /// <remarks/>
        podoba,

        /// <remarks/>
        postaveni,

        /// <remarks/>
        skutecnymMajitelemOd,

        /// <remarks/>
        spoluvlastnictvi,

        /// <remarks/>
        spravce,

        /// <remarks/>
        strukturaVztahu,

        /// <remarks/>
        text,

        /// <remarks/>
        textZaOsobu,

        /// <remarks/>
        textZruseni,

        /// <remarks/>
        typ,

        /// <remarks/>
        vlastniPodilNaHlasovani,

        /// <remarks/>
        vlastniPodilNaProspechu,
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajPodudajeUdajUdajTyp
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
    public partial class xmlSubjektUdajPodudajeUdajOsoba
    {

        private string osobaTextField;

        private string nazevField;

        private string regCisloField;

        private object euidField;

        private string icoField;

        private bool icoFieldSpecified;

        private string jmenoField;

        private string prijmeniField;

        private System.DateTime narozDatumField;

        private bool narozDatumFieldSpecified;

        private string titulPredField;

        private string titulZaField;

        /// <remarks/>
        public string osobaText
        {
            get
            {
                return this.osobaTextField;
            }
            set
            {
                this.osobaTextField = value;
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
        public string regCislo
        {
            get
            {
                return this.regCisloField;
            }
            set
            {
                this.regCisloField = value;
            }
        }

        /// <remarks/>
        public object euid
        {
            get
            {
                return this.euidField;
            }
            set
            {
                this.euidField = value;
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool icoSpecified
        {
            get
            {
                return this.icoFieldSpecified;
            }
            set
            {
                this.icoFieldSpecified = value;
            }
        }

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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool narozDatumSpecified
        {
            get
            {
                return this.narozDatumFieldSpecified;
            }
            set
            {
                this.narozDatumFieldSpecified = value;
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
    public partial class xmlSubjektUdajPodudajeUdajAdresa
    {

        private string statNazevField;

        private string adresaTextField;

        private string obecField;

        private string castObceField;

        private string cisloEvField;

        private bool cisloEvFieldSpecified;

        private string uliceField;

        private string cisloTextField;

        private string cisloPoField;

        private bool cisloPoFieldSpecified;

        private string cisloOrField;

        private string pscField;

        private object doplnujiciTextField;

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

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool cisloEvSpecified
        {
            get
            {
                return this.cisloEvFieldSpecified;
            }
            set
            {
                this.cisloEvFieldSpecified = value;
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
        public string cisloText
        {
            get
            {
                return this.cisloTextField;
            }
            set
            {
                this.cisloTextField = value;
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool cisloPoSpecified
        {
            get
            {
                return this.cisloPoFieldSpecified;
            }
            set
            {
                this.cisloPoFieldSpecified = value;
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
        public object doplnujiciText
        {
            get
            {
                return this.doplnujiciTextField;
            }
            set
            {
                this.doplnujiciTextField = value;
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
    public partial class xmlSubjektUdajPodudajeUdajUdaj
    {

        private string hlavickaField;

        private System.DateTime zapisDatumField;

        private System.DateTime vymazDatumField;

        private bool vymazDatumFieldSpecified;

        private string hodnotaTextField;

        private xmlSubjektUdajPodudajeUdajUdajHodnotaUdaje hodnotaUdajeField;

        private xmlSubjektUdajPodudajeUdajUdajUdajTyp udajTypField;

        private xmlSubjektUdajPodudajeUdajUdajOsoba osobaField;

        private xmlSubjektUdajPodudajeUdajUdajAdresa adresaField;

        private xmlSubjektUdajPodudajeUdajUdajUdaj[] podudajeField;

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
        public xmlSubjektUdajPodudajeUdajUdajHodnotaUdaje hodnotaUdaje
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
        public xmlSubjektUdajPodudajeUdajUdajUdajTyp udajTyp
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
        public xmlSubjektUdajPodudajeUdajUdajOsoba osoba
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
        public xmlSubjektUdajPodudajeUdajUdajAdresa adresa
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
        [System.Xml.Serialization.XmlArrayItemAttribute("Udaj", IsNullable = false)]
        public xmlSubjektUdajPodudajeUdajUdajUdaj[] podudaje
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
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajPodudajeUdajUdajHodnotaUdaje
    {

        private xmlSubjektUdajPodudajeUdajUdajHodnotaUdajeTypZapisu typZapisuField;

        private string textField;

        private string tField;

        private string druhPodiluField;

        private xmlSubjektUdajPodudajeUdajUdajHodnotaUdajeVklad vkladField;

        private xmlSubjektUdajPodudajeUdajUdajHodnotaUdajeSouhrn souhrnField;

        private string kmenovyListField;

        private xmlSubjektUdajPodudajeUdajUdajHodnotaUdajeSplaceni splaceniField;

        /// <remarks/>
        public xmlSubjektUdajPodudajeUdajUdajHodnotaUdajeTypZapisu typZapisu
        {
            get
            {
                return this.typZapisuField;
            }
            set
            {
                this.typZapisuField = value;
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
        public string druhPodilu
        {
            get
            {
                return this.druhPodiluField;
            }
            set
            {
                this.druhPodiluField = value;
            }
        }

        /// <remarks/>
        public xmlSubjektUdajPodudajeUdajUdajHodnotaUdajeVklad vklad
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
        public xmlSubjektUdajPodudajeUdajUdajHodnotaUdajeSouhrn souhrn
        {
            get
            {
                return this.souhrnField;
            }
            set
            {
                this.souhrnField = value;
            }
        }

        /// <remarks/>
        public string kmenovyList
        {
            get
            {
                return this.kmenovyListField;
            }
            set
            {
                this.kmenovyListField = value;
            }
        }

        /// <remarks/>
        public xmlSubjektUdajPodudajeUdajUdajHodnotaUdajeSplaceni splaceni
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
    public partial class xmlSubjektUdajPodudajeUdajUdajHodnotaUdajeTypZapisu
    {

        private xmlSubjektUdajPodudajeUdajUdajHodnotaUdajeTypZapisuID idField;

        private bool aktivniField;

        private string keyField;

        private string nazevField;

        private string ciselnikClassField;

        private xmlSubjektUdajPodudajeUdajUdajHodnotaUdajeTypZapisuExternalCodes externalCodesField;

        /// <remarks/>
        public xmlSubjektUdajPodudajeUdajUdajHodnotaUdajeTypZapisuID id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        public bool aktivni
        {
            get
            {
                return this.aktivniField;
            }
            set
            {
                this.aktivniField = value;
            }
        }

        /// <remarks/>
        public string key
        {
            get
            {
                return this.keyField;
            }
            set
            {
                this.keyField = value;
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
        public string ciselnikClass
        {
            get
            {
                return this.ciselnikClassField;
            }
            set
            {
                this.ciselnikClassField = value;
            }
        }

        /// <remarks/>
        public xmlSubjektUdajPodudajeUdajUdajHodnotaUdajeTypZapisuExternalCodes externalCodes
        {
            get
            {
                return this.externalCodesField;
            }
            set
            {
                this.externalCodesField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajPodudajeUdajUdajHodnotaUdajeTypZapisuID
    {

        private string idField;

        /// <remarks/>
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajPodudajeUdajUdajHodnotaUdajeTypZapisuExternalCodes
    {

        private string intField;

        /// <remarks/>
        public string @int
        {
            get
            {
                return this.intField;
            }
            set
            {
                this.intField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajPodudajeUdajUdajHodnotaUdajeVklad
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
    public partial class xmlSubjektUdajPodudajeUdajUdajHodnotaUdajeSouhrn
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
    public partial class xmlSubjektUdajPodudajeUdajUdajHodnotaUdajeSplaceni
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
    public partial class xmlSubjektUdajPodudajeUdajUdajUdajTyp
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
    public partial class xmlSubjektUdajPodudajeUdajUdajOsoba
    {

        private string nazevField;

        private string icoField;

        private bool icoFieldSpecified;

        private string jmenoField;

        private string prijmeniField;

        private System.DateTime narozDatumField;

        private bool narozDatumFieldSpecified;

        private string titulPredField;

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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool icoSpecified
        {
            get
            {
                return this.icoFieldSpecified;
            }
            set
            {
                this.icoFieldSpecified = value;
            }
        }

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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool narozDatumSpecified
        {
            get
            {
                return this.narozDatumFieldSpecified;
            }
            set
            {
                this.narozDatumFieldSpecified = value;
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
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajPodudajeUdajUdajAdresa
    {

        private string statNazevField;

        private string obecField;

        private string castObceField;

        private string uliceField;

        private string cisloPoField;

        private string cisloOrField;

        private string pscField;

        private object doplnujiciTextField;

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
        public object doplnujiciText
        {
            get
            {
                return this.doplnujiciTextField;
            }
            set
            {
                this.doplnujiciTextField = value;
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
    public partial class xmlSubjektUdajPodudajeUdajUdajUdaj
    {

        private string hlavickaField;

        private System.DateTime zapisDatumField;

        private System.DateTime vymazDatumField;

        private xmlSubjektUdajPodudajeUdajUdajUdajHodnotaUdaje hodnotaUdajeField;

        private xmlSubjektUdajPodudajeUdajUdajUdajUdajTyp udajTypField;

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
        public xmlSubjektUdajPodudajeUdajUdajUdajHodnotaUdaje hodnotaUdaje
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
        public xmlSubjektUdajPodudajeUdajUdajUdajUdajTyp udajTyp
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
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajPodudajeUdajUdajUdajHodnotaUdaje
    {

        private string textField;

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
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajPodudajeUdajUdajUdajUdajTyp
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
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    public enum ItemsChoiceType2
    {

        /// <remarks/>
        adresa,

        /// <remarks/>
        hlavicka,

        /// <remarks/>
        hodnotaText,

        /// <remarks/>
        hodnotaUdaje,

        /// <remarks/>
        podudaje,

        /// <remarks/>
        pravniForma,

        /// <remarks/>
        spisZn,

        /// <remarks/>
        udajTyp,

        /// <remarks/>
        vymazDatum,

        /// <remarks/>
        zapisDatum,
    }





}

