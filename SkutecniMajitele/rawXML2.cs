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
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false, ElementName = "xml")]
    public partial class rawXMLFull
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

        private xmlSubjektUdajUdajUdaj[] podudajeField;

        private xmlSubjektUdajUdajBydliste bydlisteField;

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

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Udaj", IsNullable = false)]
        public xmlSubjektUdajUdajUdaj[] podudaje
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
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajUdajHodnotaUdaje
    {

        private object[] itemsField;

        private ItemsChoiceType[] itemsElementNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("T", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("autoprupis", typeof(bool))]
        [System.Xml.Serialization.XmlElementAttribute("hodnota", typeof(xmlSubjektUdajUdajHodnotaUdajeHodnota))]
        [System.Xml.Serialization.XmlElementAttribute("jednaVeShodeSOsoby", typeof(object))]
        [System.Xml.Serialization.XmlElementAttribute("koncovyPrijemceText", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("nahradniSMOsoby", typeof(object))]
        [System.Xml.Serialization.XmlElementAttribute("pocet", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("pocetClenu", typeof(byte))]
        [System.Xml.Serialization.XmlElementAttribute("podilNaHlasovani", typeof(xmlSubjektUdajUdajHodnotaUdajePodilNaHlasovani))]
        [System.Xml.Serialization.XmlElementAttribute("podilNaProspechu", typeof(xmlSubjektUdajUdajHodnotaUdajePodilNaProspechu))]
        [System.Xml.Serialization.XmlElementAttribute("podoba", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("postaveni", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("skutecnymMajitelemOd", typeof(System.DateTime), DataType = "date")]
        [System.Xml.Serialization.XmlElementAttribute("spravce", typeof(bool))]
        [System.Xml.Serialization.XmlElementAttribute("strukturaVztahu", typeof(xmlSubjektUdajUdajHodnotaUdajeStrukturaVztahu))]
        [System.Xml.Serialization.XmlElementAttribute("text", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("textZaOsobu", typeof(xmlSubjektUdajUdajHodnotaUdajeTextZaOsobu))]
        [System.Xml.Serialization.XmlElementAttribute("textZruseni", typeof(object))]
        [System.Xml.Serialization.XmlElementAttribute("typ", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("vklad", typeof(xmlSubjektUdajUdajHodnotaUdajeVklad))]
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

        private byte textValueField;

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
        public byte textValue
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
    public partial class xmlSubjektUdajUdajHodnotaUdajeTextZaOsobu
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
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajUdajHodnotaUdajeVklad
    {

        private xmlSubjektUdajUdajHodnotaUdajeVkladVklad vkladField;

        private xmlSubjektUdajUdajHodnotaUdajeVkladSouhrn souhrnField;

        private xmlSubjektUdajUdajHodnotaUdajeVkladSplaceni splaceniField;

        /// <remarks/>
        public xmlSubjektUdajUdajHodnotaUdajeVkladVklad vklad
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
        public xmlSubjektUdajUdajHodnotaUdajeVkladSouhrn souhrn
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
        public xmlSubjektUdajUdajHodnotaUdajeVkladSplaceni splaceni
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
    public partial class xmlSubjektUdajUdajHodnotaUdajeVkladVklad
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
    public partial class xmlSubjektUdajUdajHodnotaUdajeVkladSouhrn
    {

        private string typField;

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
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajUdajHodnotaUdajeVkladSplaceni
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
        vklad,

        /// <remarks/>
        vlastniPodilNaHlasovani,

        /// <remarks/>
        vlastniPodilNaProspechu,
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
    public partial class xmlSubjektUdajUdajAdresa
    {

        private object[] itemsField;

        private ItemsChoiceType1[] itemsElementNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("adresaText", typeof(string))]
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
        adresaText,

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
    public partial class xmlSubjektUdajUdajUdaj
    {

        private string hlavickaField;

        private System.DateTime zapisDatumField;

        private System.DateTime vymazDatumField;

        private bool vymazDatumFieldSpecified;

        private string hodnotaTextField;

        private xmlSubjektUdajUdajUdajHodnotaUdaje hodnotaUdajeField;

        private xmlSubjektUdajUdajUdajUdajTyp udajTypField;

        private xmlSubjektUdajUdajUdajUdaj[] podudajeField;

        private xmlSubjektUdajUdajUdajOsoba osobaField;

        private xmlSubjektUdajUdajUdajAdresa adresaField;

        private xmlSubjektUdajUdajUdajBydliste bydlisteField;

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
        public xmlSubjektUdajUdajUdajHodnotaUdaje hodnotaUdaje
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
        public xmlSubjektUdajUdajUdajUdajTyp udajTyp
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
        public xmlSubjektUdajUdajUdajUdaj[] podudaje
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
        public xmlSubjektUdajUdajUdajOsoba osoba
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
        public xmlSubjektUdajUdajUdajAdresa adresa
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
        public xmlSubjektUdajUdajUdajBydliste bydliste
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
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajUdajUdajHodnotaUdaje
    {

        private string datRozhodnutihOsField;

        private string spisZnOsField;

        private string datVyveseniField;

        private bool spravceField;

        private bool spravceFieldSpecified;

        private string druhPodiluField;

        private xmlSubjektUdajUdajUdajHodnotaUdajeVklad vkladField;

        private xmlSubjektUdajUdajUdajHodnotaUdajeSouhrn souhrnField;

        private string kmenovyListField;

        private xmlSubjektUdajUdajUdajHodnotaUdajeSplaceni splaceniField;

        private xmlSubjektUdajUdajUdajHodnotaUdajeTypZapisu typZapisuField;

        private string textField;

        private string tField;

        private xmlSubjektUdajUdajUdajHodnotaUdajeTextZaOsobu textZaOsobuField;

        private xmlSubjektUdajUdajUdajHodnotaUdajeTextZruseni textZruseniField;

        /// <remarks/>
        public string datRozhodnutihOs
        {
            get
            {
                return this.datRozhodnutihOsField;
            }
            set
            {
                this.datRozhodnutihOsField = value;
            }
        }

        /// <remarks/>
        public string spisZnOs
        {
            get
            {
                return this.spisZnOsField;
            }
            set
            {
                this.spisZnOsField = value;
            }
        }

        /// <remarks/>
        public string datVyveseni
        {
            get
            {
                return this.datVyveseniField;
            }
            set
            {
                this.datVyveseniField = value;
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
        public xmlSubjektUdajUdajUdajHodnotaUdajeVklad vklad
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
        public xmlSubjektUdajUdajUdajHodnotaUdajeSouhrn souhrn
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
        public xmlSubjektUdajUdajUdajHodnotaUdajeSplaceni splaceni
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

        /// <remarks/>
        public xmlSubjektUdajUdajUdajHodnotaUdajeTypZapisu typZapisu
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
        public xmlSubjektUdajUdajUdajHodnotaUdajeTextZaOsobu textZaOsobu
        {
            get
            {
                return this.textZaOsobuField;
            }
            set
            {
                this.textZaOsobuField = value;
            }
        }

        /// <remarks/>
        public xmlSubjektUdajUdajUdajHodnotaUdajeTextZruseni textZruseni
        {
            get
            {
                return this.textZruseniField;
            }
            set
            {
                this.textZruseniField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajUdajUdajHodnotaUdajeVklad
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
    public partial class xmlSubjektUdajUdajUdajHodnotaUdajeSouhrn
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
    public partial class xmlSubjektUdajUdajUdajHodnotaUdajeSplaceni
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
    public partial class xmlSubjektUdajUdajUdajHodnotaUdajeTypZapisu
    {

        private xmlSubjektUdajUdajUdajHodnotaUdajeTypZapisuID idField;

        private bool aktivniField;

        private string keyField;

        private string nazevField;

        private string ciselnikClassField;

        private xmlSubjektUdajUdajUdajHodnotaUdajeTypZapisuExternalCodes externalCodesField;

        /// <remarks/>
        public xmlSubjektUdajUdajUdajHodnotaUdajeTypZapisuID id
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
        public xmlSubjektUdajUdajUdajHodnotaUdajeTypZapisuExternalCodes externalCodes
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
    public partial class xmlSubjektUdajUdajUdajHodnotaUdajeTypZapisuID
    {

        private byte idField;

        /// <remarks/>
        public byte id
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
    public partial class xmlSubjektUdajUdajUdajHodnotaUdajeTypZapisuExternalCodes
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
    public partial class xmlSubjektUdajUdajUdajHodnotaUdajeTextZaOsobu
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
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajUdajUdajHodnotaUdajeTextZruseni
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
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajUdajUdajUdajTyp
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
    public partial class xmlSubjektUdajUdajUdajUdaj
    {

        private string hlavickaField;

        private System.DateTime zapisDatumField;

        private System.DateTime vymazDatumField;

        private bool vymazDatumFieldSpecified;

        private string hodnotaTextField;

        private xmlSubjektUdajUdajUdajUdajHodnotaUdaje hodnotaUdajeField;

        private xmlSubjektUdajUdajUdajUdajUdajTyp udajTypField;

        private xmlSubjektUdajUdajUdajUdajOsoba osobaField;

        private xmlSubjektUdajUdajUdajUdajAdresa adresaField;

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
        public xmlSubjektUdajUdajUdajUdajHodnotaUdaje hodnotaUdaje
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
        public xmlSubjektUdajUdajUdajUdajUdajTyp udajTyp
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
        public xmlSubjektUdajUdajUdajUdajOsoba osoba
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
        public xmlSubjektUdajUdajUdajUdajAdresa adresa
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
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajUdajUdajUdajHodnotaUdaje
    {

        private string tField;

        private object textZaOsobuField;

        private object textZruseniField;

        private string vznikZastavnihoPravaField;

        private string textField;

        private string zanikZastavnihoPravaField;

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
        public object textZaOsobu
        {
            get
            {
                return this.textZaOsobuField;
            }
            set
            {
                this.textZaOsobuField = value;
            }
        }

        /// <remarks/>
        public object textZruseni
        {
            get
            {
                return this.textZruseniField;
            }
            set
            {
                this.textZruseniField = value;
            }
        }

        /// <remarks/>
        public string vznikZastavnihoPrava
        {
            get
            {
                return this.vznikZastavnihoPravaField;
            }
            set
            {
                this.vznikZastavnihoPravaField = value;
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
        public string zanikZastavnihoPrava
        {
            get
            {
                return this.zanikZastavnihoPravaField;
            }
            set
            {
                this.zanikZastavnihoPravaField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class xmlSubjektUdajUdajUdajUdajUdajTyp
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
    public partial class xmlSubjektUdajUdajUdajUdajOsoba
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
    public partial class xmlSubjektUdajUdajUdajUdajAdresa
    {

        private object[] itemsField;

        private ItemsChoiceType2[] itemsElementNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("castObce", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("cisloPo", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("cisloText", typeof(string))]
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
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    public enum ItemsChoiceType2
    {

        /// <remarks/>
        castObce,

        /// <remarks/>
        cisloPo,

        /// <remarks/>
        cisloText,

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
    public partial class xmlSubjektUdajUdajUdajOsoba
    {

        private string nazevField;

        private string icoField;

        private bool icoFieldSpecified;

        private string jmenoField;

        private string prijmeniField;

        private System.DateTime narozDatumField;

        private bool narozDatumFieldSpecified;

        private string titulPredField;

        private string titulZaField;

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
    public partial class xmlSubjektUdajUdajUdajAdresa
    {

        private string statNazevField;

        private string obecField;

        private string castObceField;

        private string uliceField;

        private string cisloTextField;

        private string cisloPoField;

        private bool cisloPoFieldSpecified;

        private string cisloOrField;

        private string pscField;

        private bool pscFieldSpecified;

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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool pscSpecified
        {
            get
            {
                return this.pscFieldSpecified;
            }
            set
            {
                this.pscFieldSpecified = value;
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
    public partial class xmlSubjektUdajUdajUdajBydliste
    {

        private string statNazevField;

        private string obecField;

        private string castObceField;

        private string uliceField;

        private string cisloPoField;

        private byte cisloOrField;

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
        public byte cisloOr
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

        private string cisloTextField;

        private bool cisloTextFieldSpecified;

        private string cisloPoField;

        private bool cisloPoFieldSpecified;

        private byte cisloOrField;

        private bool cisloOrFieldSpecified;

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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool cisloTextSpecified
        {
            get
            {
                return this.cisloTextFieldSpecified;
            }
            set
            {
                this.cisloTextFieldSpecified = value;
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
        public byte cisloOr
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool cisloOrSpecified
        {
            get
            {
                return this.cisloOrFieldSpecified;
            }
            set
            {
                this.cisloOrFieldSpecified = value;
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

        private object[] itemsField;

        private ItemsChoiceType3[] itemsElementNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("adresaText", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("castObce", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("cisloEv", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("cisloOr", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("cisloPo", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("cisloText", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("doplnujiciText", typeof(string))]
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
        public ItemsChoiceType3[] ItemsElementName
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
    public enum ItemsChoiceType3
    {

        /// <remarks/>
        adresaText,

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

