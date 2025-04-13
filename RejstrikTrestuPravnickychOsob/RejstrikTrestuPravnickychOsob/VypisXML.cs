namespace RejstrikTrestuPravnickychOsob
{

    public class VypisXML
    {

        // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://po.rt.cleverlance.com/vydanyDokument_1.0")]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://po.rt.cleverlance.com/vydanyDokument_1.0", IsNullable = false)]
        public partial class vypisList
        {

            private Vypis[] vypisField;

            private decimal verzeField;

            private System.DateTime datumVystaveniField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("vypis")]
            public Vypis[] vypis
            {
                get
                {
                    return this.vypisField;
                }
                set
                {
                    this.vypisField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public decimal verze
            {
                get
                {
                    return this.verzeField;
                }
                set
                {
                    this.verzeField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public System.DateTime datumVystaveni
            {
                get
                {
                    return this.datumVystaveniField;
                }
                set
                {
                    this.datumVystaveniField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://po.rt.cleverlance.com/vydanyDokument_1.0")]
        public partial class Vypis
        {

            private vypisListVypisOsobaPravnicka osobaPravnickaField;

            private vypisListVypisZaznamy zaznamyField;

            /// <remarks/>
            public vypisListVypisOsobaPravnicka osobaPravnicka
            {
                get
                {
                    return this.osobaPravnickaField;
                }
                set
                {
                    this.osobaPravnickaField = value;
                }
            }

            /// <remarks/>
            public vypisListVypisZaznamy zaznamy
            {
                get
                {
                    return this.zaznamyField;
                }
                set
                {
                    this.zaznamyField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://po.rt.cleverlance.com/vydanyDokument_1.0")]
        public partial class vypisListVypisOsobaPravnicka
        {

            private osobaPravnickaCeska osobaPravnickaCeskaField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://po.rt.cleverlance.com/commons_1.0")]
            public osobaPravnickaCeska osobaPravnickaCeska
            {
                get
                {
                    return this.osobaPravnickaCeskaField;
                }
                set
                {
                    this.osobaPravnickaCeskaField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://po.rt.cleverlance.com/commons_1.0")]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://po.rt.cleverlance.com/commons_1.0", IsNullable = false)]
        public partial class osobaPravnickaCeska
        {

            private string nazevField;

            private string sidloField;

            private uint icoField;

            private uint sidloRuianCodeField;

            private bool sidloRuianCodeFieldSpecified;

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
            public string sidlo
            {
                get
                {
                    return this.sidloField;
                }
                set
                {
                    this.sidloField = value;
                }
            }

            /// <remarks/>
            public uint ico
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
            public uint sidloRuianCode
            {
                get
                {
                    return this.sidloRuianCodeField;
                }
                set
                {
                    this.sidloRuianCodeField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlIgnoreAttribute()]
            public bool sidloRuianCodeSpecified
            {
                get
                {
                    return this.sidloRuianCodeFieldSpecified;
                }
                set
                {
                    this.sidloRuianCodeFieldSpecified = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://po.rt.cleverlance.com/commonsOdsouzeni_1.0")]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://po.rt.cleverlance.com/commonsOdsouzeni_1.0", IsNullable = false)]
        public partial class paragraf
        {

            private paragrafParagrafCisel paragrafCiselField;

            /// <remarks/>
            public paragrafParagrafCisel paragrafCisel
            {
                get
                {
                    return this.paragrafCiselField;
                }
                set
                {
                    this.paragrafCiselField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://po.rt.cleverlance.com/commonsOdsouzeni_1.0")]
        public partial class paragrafParagrafCisel
        {

            private string paragrafCisloField;

            private string odstavecPismenoField;

            private paragrafParagrafCiselZakon zakonField;

            private paragrafParagrafCiselParagrafDoplnek[] paragrafyDoplnekField;

            private paragrafParagrafCiselZavineni zavineniField;

            /// <remarks/>
            public string paragrafCislo
            {
                get
                {
                    return this.paragrafCisloField;
                }
                set
                {
                    this.paragrafCisloField = value;
                }
            }

            /// <remarks/>
            public string odstavecPismeno
            {
                get
                {
                    return this.odstavecPismenoField;
                }
                set
                {
                    this.odstavecPismenoField = value;
                }
            }

            /// <remarks/>
            public paragrafParagrafCiselZakon zakon
            {
                get
                {
                    return this.zakonField;
                }
                set
                {
                    this.zakonField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlArrayItemAttribute("paragrafDoplnek", IsNullable = false)]
            public paragrafParagrafCiselParagrafDoplnek[] paragrafyDoplnek
            {
                get
                {
                    return this.paragrafyDoplnekField;
                }
                set
                {
                    this.paragrafyDoplnekField = value;
                }
            }

            /// <remarks/>
            public paragrafParagrafCiselZavineni zavineni
            {
                get
                {
                    return this.zavineniField;
                }
                set
                {
                    this.zavineniField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://po.rt.cleverlance.com/commonsOdsouzeni_1.0")]
        public partial class paragrafParagrafCiselZakon
        {

            private string zkratkaField;

            private string valueField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
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

            /// <remarks/>
            [System.Xml.Serialization.XmlTextAttribute()]
            public string Value
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
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://po.rt.cleverlance.com/commonsOdsouzeni_1.0")]
        public partial class paragrafParagrafCiselParagrafDoplnek
        {

            private byte paragrafCisloField;

            private string odstavecPismenoField;

            private string typField;

            /// <remarks/>
            public byte paragrafCislo
            {
                get
                {
                    return this.paragrafCisloField;
                }
                set
                {
                    this.paragrafCisloField = value;
                }
            }

            /// <remarks/>
            public string odstavecPismeno
            {
                get
                {
                    return this.odstavecPismenoField;
                }
                set
                {
                    this.odstavecPismenoField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
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

            public override string ToString()
            {
                var s = $"{this.typ} dle §{this.paragrafCislo} {(string.IsNullOrWhiteSpace(this.odstavecPismeno) ? "" : "odst. " + this.odstavecPismeno)}";
                return s.Trim();
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://po.rt.cleverlance.com/commonsOdsouzeni_1.0")]
        public partial class paragrafParagrafCiselZavineni
        {

            private string typField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
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
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://po.rt.cleverlance.com/vydanyDokument_1.0")]
        public partial class vypisListVypisZaznamy
        {

            private vypisListVypisZaznamyZaznam zaznamField;

            /// <remarks/>
            public vypisListVypisZaznamyZaznam zaznam
            {
                get
                {
                    return this.zaznamField;
                }
                set
                {
                    this.zaznamField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://po.rt.cleverlance.com/vydanyDokument_1.0")]
        public partial class vypisListVypisZaznamyZaznam
        {

            private vypisListVypisZaznamyZaznamOdsouzeni odsouzeniField;

            private paragraf[] paragrafyField;

            private vypisListVypisZaznamyZaznamTrest[] trestyField;

            /// <remarks/>
            public vypisListVypisZaznamyZaznamOdsouzeni odsouzeni
            {
                get
                {
                    return this.odsouzeniField;
                }
                set
                {
                    this.odsouzeniField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlArrayItemAttribute("paragraf", Namespace = "http://po.rt.cleverlance.com/commonsOdsouzeni_1.0", IsNullable = false)]
            public paragraf[] paragrafy
            {
                get
                {
                    return this.paragrafyField;
                }
                set
                {
                    this.paragrafyField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlArrayItemAttribute("trest", IsNullable = false)]
            public vypisListVypisZaznamyZaznamTrest[] tresty
            {
                get
                {
                    return this.trestyField;
                }
                set
                {
                    this.trestyField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://po.rt.cleverlance.com/vydanyDokument_1.0")]
        public partial class vypisListVypisZaznamyZaznamOdsouzeni
        {

            private object[] itemsField;

            private ItemsChoiceType[] itemsElementNameField;

            private bool dohodaVinaTrestField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("datumPM", typeof(System.DateTime), DataType = "date")]
            [System.Xml.Serialization.XmlElementAttribute("datumRozhodnuti", typeof(System.DateTime), DataType = "date")]
            [System.Xml.Serialization.XmlElementAttribute("druhRozhodnuti", typeof(vypisListVypisZaznamyZaznamOdsouzeniDruhRozhodnuti))]
            [System.Xml.Serialization.XmlElementAttribute("odvolaci", typeof(vypisListVypisZaznamyZaznamOdsouzeniOdvolaci))]
            [System.Xml.Serialization.XmlElementAttribute("organizace", typeof(string))]
            [System.Xml.Serialization.XmlElementAttribute("osobaPravnickaPuvodni", typeof(string))]
            [System.Xml.Serialization.XmlElementAttribute("souvisejici", typeof(vypisListVypisZaznamyZaznamOdsouzeniSouvisejici))]
            [System.Xml.Serialization.XmlElementAttribute("spisZnacka", typeof(string))]
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

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public bool dohodaVinaTrest
            {
                get
                {
                    return this.dohodaVinaTrestField;
                }
                set
                {
                    this.dohodaVinaTrestField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://po.rt.cleverlance.com/vydanyDokument_1.0")]
        public partial class vypisListVypisZaznamyZaznamOdsouzeniDruhRozhodnuti
        {

            private string druhField;

            private System.DateTime datumTPField;

            private bool datumTPFieldSpecified;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string druh
            {
                get
                {
                    return this.druhField;
                }
                set
                {
                    this.druhField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
            public System.DateTime datumTP
            {
                get
                {
                    return this.datumTPField;
                }
                set
                {
                    this.datumTPField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlIgnoreAttribute()]
            public bool datumTPSpecified
            {
                get
                {
                    return this.datumTPFieldSpecified;
                }
                set
                {
                    this.datumTPFieldSpecified = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://po.rt.cleverlance.com/vydanyDokument_1.0")]
        public partial class vypisListVypisZaznamyZaznamOdsouzeniOdvolaci
        {

            private druhRozhodnuti druhRozhodnutiField;

            private System.DateTime datumRozhodnutiField;

            private string spisZnackaField;

            private string organizaceField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://po.rt.cleverlance.com/commonsOdsouzeni_1.0")]
            public druhRozhodnuti druhRozhodnuti
            {
                get
                {
                    return this.druhRozhodnutiField;
                }
                set
                {
                    this.druhRozhodnutiField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://po.rt.cleverlance.com/commonsOdsouzeni_1.0", DataType = "date")]
            public System.DateTime datumRozhodnuti
            {
                get
                {
                    return this.datumRozhodnutiField;
                }
                set
                {
                    this.datumRozhodnutiField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://po.rt.cleverlance.com/commonsOdsouzeni_1.0")]
            public string spisZnacka
            {
                get
                {
                    return this.spisZnackaField;
                }
                set
                {
                    this.spisZnackaField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://po.rt.cleverlance.com/commonsOdsouzeni_1.0")]
            public string organizace
            {
                get
                {
                    return this.organizaceField;
                }
                set
                {
                    this.organizaceField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://po.rt.cleverlance.com/commonsOdsouzeni_1.0")]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://po.rt.cleverlance.com/commonsOdsouzeni_1.0", IsNullable = false)]
        public partial class druhRozhodnuti
        {

            private string druhField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string druh
            {
                get
                {
                    return this.druhField;
                }
                set
                {
                    this.druhField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://po.rt.cleverlance.com/vydanyDokument_1.0")]
        public partial class vypisListVypisZaznamyZaznamOdsouzeniSouvisejici
        {

            private souvislost[] souvislostField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("souvislost", Namespace = "http://po.rt.cleverlance.com/commonsOdsouzeni_1.0")]
            public souvislost[] souvislost
            {
                get
                {
                    return this.souvislostField;
                }
                set
                {
                    this.souvislostField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://po.rt.cleverlance.com/commonsOdsouzeni_1.0")]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://po.rt.cleverlance.com/commonsOdsouzeni_1.0", IsNullable = false)]
        public partial class souvislost
        {

            private souvislostTyp typField;

            private string spisZnackaField;

            private souvislostDruhRozhodnuti druhRozhodnutiField;

            private System.DateTime datumRozhodnutiField;

            private string organizaceField;

            /// <remarks/>
            public souvislostTyp typ
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
            public string spisZnacka
            {
                get
                {
                    return this.spisZnackaField;
                }
                set
                {
                    this.spisZnackaField = value;
                }
            }

            /// <remarks/>
            public souvislostDruhRozhodnuti druhRozhodnuti
            {
                get
                {
                    return this.druhRozhodnutiField;
                }
                set
                {
                    this.druhRozhodnutiField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(DataType = "dateTime")]
            public System.DateTime datumRozhodnuti
            {
                get
                {
                    return this.datumRozhodnutiField;
                }
                set
                {
                    this.datumRozhodnutiField = value;
                }
            }

            /// <remarks/>
            public string organizace
            {
                get
                {
                    return this.organizaceField;
                }
                set
                {
                    this.organizaceField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://po.rt.cleverlance.com/commonsOdsouzeni_1.0")]
        public partial class souvislostTyp
        {

            private string zkratkaField;

            private string valueField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
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

            /// <remarks/>
            [System.Xml.Serialization.XmlTextAttribute()]
            public string Value
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
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://po.rt.cleverlance.com/commonsOdsouzeni_1.0")]
        public partial class souvislostDruhRozhodnuti
        {

            private string druhField;

            private System.DateTime datumTPField;

            private bool datumTPFieldSpecified;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string druh
            {
                get
                {
                    return this.druhField;
                }
                set
                {
                    this.druhField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
            public System.DateTime datumTP
            {
                get
                {
                    return this.datumTPField;
                }
                set
                {
                    this.datumTPField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlIgnoreAttribute()]
            public bool datumTPSpecified
            {
                get
                {
                    return this.datumTPFieldSpecified;
                }
                set
                {
                    this.datumTPFieldSpecified = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://po.rt.cleverlance.com/vydanyDokument_1.0", IncludeInSchema = false)]
        public enum ItemsChoiceType
        {

            /// <remarks/>
            datumPM,

            /// <remarks/>
            datumRozhodnuti,

            /// <remarks/>
            druhRozhodnuti,

            /// <remarks/>
            odvolaci,

            /// <remarks/>
            organizace,

            /// <remarks/>
            osobaPravnickaPuvodni,

            /// <remarks/>
            souvisejici,

            /// <remarks/>
            spisZnacka,
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://po.rt.cleverlance.com/vydanyDokument_1.0")]
        public partial class vypisListVypisZaznamyZaznamTrest
        {

            private object[] itemsField;

            private bool zmenaField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("druh", typeof(druh), Namespace = "http://po.rt.cleverlance.com/commonsOdsouzeni_1.0")]
            [System.Xml.Serialization.XmlElementAttribute("prubehy", typeof(prubehy), Namespace = "http://po.rt.cleverlance.com/commonsOdsouzeni_1.0")]
            [System.Xml.Serialization.XmlElementAttribute("vymery", typeof(vymery), Namespace = "http://po.rt.cleverlance.com/commonsOdsouzeni_1.0")]
            [System.Xml.Serialization.XmlElementAttribute("poznamka", typeof(string))]
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
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public bool zmena
            {
                get
                {
                    return this.zmenaField;
                }
                set
                {
                    this.zmenaField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://po.rt.cleverlance.com/commonsOdsouzeni_1.0")]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://po.rt.cleverlance.com/commonsOdsouzeni_1.0", IsNullable = false)]
        public partial class druh
        {

            private string zkratkaField;

            private string valueField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
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

            /// <remarks/>
            [System.Xml.Serialization.XmlTextAttribute()]
            public string Value
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
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://po.rt.cleverlance.com/commonsOdsouzeni_1.0")]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://po.rt.cleverlance.com/commonsOdsouzeni_1.0", IsNullable = false)]
        public partial class prubehy
        {

            private prubehyPrubeh[] prubehField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("prubeh")]
            public prubehyPrubeh[] prubeh
            {
                get
                {
                    return this.prubehField;
                }
                set
                {
                    this.prubehField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://po.rt.cleverlance.com/commonsOdsouzeni_1.0")]
        public partial class prubehyPrubeh
        {

            private prubehyPrubehSkupina skupinaField;

            private prubehyPrubehPolozka polozkaField;

            private string hodnotaField;

            /// <remarks/>
            public prubehyPrubehSkupina skupina
            {
                get
                {
                    return this.skupinaField;
                }
                set
                {
                    this.skupinaField = value;
                }
            }

            /// <remarks/>
            public prubehyPrubehPolozka polozka
            {
                get
                {
                    return this.polozkaField;
                }
                set
                {
                    this.polozkaField = value;
                }
            }

            /// <remarks/>
            public string hodnota
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
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://po.rt.cleverlance.com/commonsOdsouzeni_1.0")]
        public partial class prubehyPrubehSkupina
        {

            private string zkratkaField;

            private string valueField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
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

            /// <remarks/>
            [System.Xml.Serialization.XmlTextAttribute()]
            public string Value
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
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://po.rt.cleverlance.com/commonsOdsouzeni_1.0")]
        public partial class prubehyPrubehPolozka
        {

            private string zkratkaField;

            private string valueField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
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

            /// <remarks/>
            [System.Xml.Serialization.XmlTextAttribute()]
            public string Value
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
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://po.rt.cleverlance.com/commonsOdsouzeni_1.0")]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://po.rt.cleverlance.com/commonsOdsouzeni_1.0", IsNullable = false)]
        public partial class vymery
        {

            private vymeryVymera[] vymeraField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("vymera")]
            public vymeryVymera[] vymera
            {
                get
                {
                    return this.vymeraField;
                }
                set
                {
                    this.vymeraField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://po.rt.cleverlance.com/commonsOdsouzeni_1.0")]
        public partial class vymeryVymera
        {

            private vymeryVymeraSkupina skupinaField;

            private vymeryVymeraPolozka polozkaField;

            private string hodnotaField;

            /// <remarks/>
            public vymeryVymeraSkupina skupina
            {
                get
                {
                    return this.skupinaField;
                }
                set
                {
                    this.skupinaField = value;
                }
            }

            /// <remarks/>
            public vymeryVymeraPolozka polozka
            {
                get
                {
                    return this.polozkaField;
                }
                set
                {
                    this.polozkaField = value;
                }
            }

            /// <remarks/>
            public string hodnota
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
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://po.rt.cleverlance.com/commonsOdsouzeni_1.0")]
        public partial class vymeryVymeraSkupina
        {

            private string zkratkaField;

            private string valueField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
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

            /// <remarks/>
            [System.Xml.Serialization.XmlTextAttribute()]
            public string Value
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
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://po.rt.cleverlance.com/commonsOdsouzeni_1.0")]
        public partial class vymeryVymeraPolozka
        {

            private string zkratkaField;

            private string valueField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
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

            /// <remarks/>
            [System.Xml.Serialization.XmlTextAttribute()]
            public string Value
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


    }
}