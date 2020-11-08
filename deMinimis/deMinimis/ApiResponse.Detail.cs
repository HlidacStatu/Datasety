using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace deMinimis.Response
{

    public class Detail
    {


        // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.pds.eu/RDM_PUB01B")]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.pds.eu/RDM_PUB01B", IsNullable = false)]
        public partial class Response
        {

            private int pocetField;

            private ResponseSubjekt[] seznam_subjektuField;

            /// <remarks/>
            public int pocet
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
            [System.Xml.Serialization.XmlArrayItemAttribute("subjekt", IsNullable = false)]
            public ResponseSubjekt[] seznam_subjektu
            {
                get
                {
                    return this.seznam_subjektuField;
                }
                set
                {
                    this.seznam_subjektuField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.pds.eu/RDM_PUB01B")]
        public partial class ResponseSubjekt
        {

            private int subjektidField;

            private string obchodni_jmenoField;

            private ResponseSubjektIdentifikator[] seznam_identifikatoruField;

            private ResponseSubjektAdresa adresaField;

            private ResponseSubjektOblast_stav[] limity_stavField;

            private ResponseSubjektNarizeni_stav[] limity_stav_narizeniField;

            private ResponseSubjektPodpora[] seznam_podporField;

            private System.DateTime datum_zahajeni_cinnostiField;

            private ResponseSubjektUcetni_obdobi[] seznam_uoField;

            /// <remarks/>
            public int subjektid
            {
                get
                {
                    return this.subjektidField;
                }
                set
                {
                    this.subjektidField = value;
                }
            }

            /// <remarks/>
            public string obchodni_jmeno
            {
                get
                {
                    return this.obchodni_jmenoField;
                }
                set
                {
                    this.obchodni_jmenoField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlArrayItemAttribute("identifikator", IsNullable = false)]
            public ResponseSubjektIdentifikator[] seznam_identifikatoru
            {
                get
                {
                    return this.seznam_identifikatoruField;
                }
                set
                {
                    this.seznam_identifikatoruField = value;
                }
            }

            /// <remarks/>
            public ResponseSubjektAdresa adresa
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
            [System.Xml.Serialization.XmlArrayItemAttribute("oblast_stav", IsNullable = false)]
            public ResponseSubjektOblast_stav[] limity_stav
            {
                get
                {
                    return this.limity_stavField;
                }
                set
                {
                    this.limity_stavField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlArrayItemAttribute("narizeni_stav", IsNullable = false)]
            public ResponseSubjektNarizeni_stav[] limity_stav_narizeni
            {
                get
                {
                    return this.limity_stav_narizeniField;
                }
                set
                {
                    this.limity_stav_narizeniField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlArrayItemAttribute("podpora", IsNullable = false)]
            public ResponseSubjektPodpora[] seznam_podpor
            {
                get
                {
                    return this.seznam_podporField;
                }
                set
                {
                    this.seznam_podporField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
            public System.DateTime datum_zahajeni_cinnosti
            {
                get
                {
                    return this.datum_zahajeni_cinnostiField;
                }
                set
                {
                    this.datum_zahajeni_cinnostiField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlArrayItemAttribute("ucetni_obdobi", IsNullable = false)]
            public ResponseSubjektUcetni_obdobi[] seznam_uo
            {
                get
                {
                    return this.seznam_uoField;
                }
                set
                {
                    this.seznam_uoField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.pds.eu/RDM_PUB01B")]
        public partial class ResponseSubjektIdentifikator
        {

            private System.DateTime platnost_odField;

            private string typField;

            private string valueField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
            public System.DateTime platnost_od
            {
                get
                {
                    return this.platnost_odField;
                }
                set
                {
                    this.platnost_odField = value;
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
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.pds.eu/RDM_PUB01B")]
        public partial class ResponseSubjektAdresa
        {

            private string ulicenazField;

            private int cisdomhodField;

            private int cisorhodField;

            private string cobcenazField;

            private string mcastnazField;

            private string obecnazField;

            private int pscField;

            private int kodField;

            /// <remarks/>
            public string ulicenaz
            {
                get
                {
                    return this.ulicenazField;
                }
                set
                {
                    this.ulicenazField = value;
                }
            }

            /// <remarks/>
            public int cisdomhod
            {
                get
                {
                    return this.cisdomhodField;
                }
                set
                {
                    this.cisdomhodField = value;
                }
            }

            /// <remarks/>
            public int cisorhod
            {
                get
                {
                    return this.cisorhodField;
                }
                set
                {
                    this.cisorhodField = value;
                }
            }

            /// <remarks/>
            public string cobcenaz
            {
                get
                {
                    return this.cobcenazField;
                }
                set
                {
                    this.cobcenazField = value;
                }
            }

            /// <remarks/>
            public string mcastnaz
            {
                get
                {
                    return this.mcastnazField;
                }
                set
                {
                    this.mcastnazField = value;
                }
            }

            /// <remarks/>
            public string obecnaz
            {
                get
                {
                    return this.obecnazField;
                }
                set
                {
                    this.obecnazField = value;
                }
            }

            /// <remarks/>
            public int psc
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
            public int kod
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
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.pds.eu/RDM_PUB01B")]
        public partial class ResponseSubjektOblast_stav
        {

            private System.DateTime platnostField;

            private string oblast_kodField;

            private int limitField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
            public System.DateTime platnost
            {
                get
                {
                    return this.platnostField;
                }
                set
                {
                    this.platnostField = value;
                }
            }

            /// <remarks/>
            public string oblast_kod
            {
                get
                {
                    return this.oblast_kodField;
                }
                set
                {
                    this.oblast_kodField = value;
                }
            }

            /// <remarks/>
            public int limit
            {
                get
                {
                    return this.limitField;
                }
                set
                {
                    this.limitField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.pds.eu/RDM_PUB01B")]
        public partial class ResponseSubjektNarizeni_stav
        {

            private System.DateTime platnostField;

            private string kodField;

            private int limitField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
            public System.DateTime platnost
            {
                get
                {
                    return this.platnostField;
                }
                set
                {
                    this.platnostField = value;
                }
            }

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
            public int limit
            {
                get
                {
                    return this.limitField;
                }
                set
                {
                    this.limitField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.pds.eu/RDM_PUB01B")]
        public partial class ResponseSubjektPodpora
        {

            private string oblastField;

            private System.DateTime datum_prideleniField;

            private string menaField;

            private decimal castka_kcField;

            private decimal castka_euroField;

            private int forma_podporyField;

            private string ucel_podporyField;

            private int pravni_akt_poskytnutiField;

            private string cj_poskytovatelField;

            private string cj_prijemceField;

            private ResponseSubjektPodporaRezim_podpory rezim_podporyField;

            private int id_podporyField;

            private int poskytovatel_idField;

            private string poskytovatel_ojmField;

            private int poskytovatel_icField;

            private int stavField;

            private ResponseSubjektPodporaSeznam_priznaky seznam_priznakyField;

            private System.DateTime insdatField;

            private System.DateTime edidatField;

            private bool edidatFieldSpecified;

            /// <remarks/>
            public string oblast
            {
                get
                {
                    return this.oblastField;
                }
                set
                {
                    this.oblastField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
            public System.DateTime datum_prideleni
            {
                get
                {
                    return this.datum_prideleniField;
                }
                set
                {
                    this.datum_prideleniField = value;
                }
            }

            /// <remarks/>
            public string mena
            {
                get
                {
                    return this.menaField;
                }
                set
                {
                    this.menaField = value;
                }
            }

            /// <remarks/>
            public decimal castka_kc
            {
                get
                {
                    return this.castka_kcField;
                }
                set
                {
                    this.castka_kcField = value;
                }
            }

            /// <remarks/>
            public decimal castka_euro
            {
                get
                {
                    return this.castka_euroField;
                }
                set
                {
                    this.castka_euroField = value;
                }
            }

            /// <remarks/>
            public int forma_podpory
            {
                get
                {
                    return this.forma_podporyField;
                }
                set
                {
                    this.forma_podporyField = value;
                }
            }

            /// <remarks/>
            public string ucel_podpory
            {
                get
                {
                    return this.ucel_podporyField;
                }
                set
                {
                    this.ucel_podporyField = value;
                }
            }

            /// <remarks/>
            public int pravni_akt_poskytnuti
            {
                get
                {
                    return this.pravni_akt_poskytnutiField;
                }
                set
                {
                    this.pravni_akt_poskytnutiField = value;
                }
            }

            /// <remarks/>
            public string cj_poskytovatel
            {
                get
                {
                    return this.cj_poskytovatelField;
                }
                set
                {
                    this.cj_poskytovatelField = value;
                }
            }

            /// <remarks/>
            public string cj_prijemce
            {
                get
                {
                    return this.cj_prijemceField;
                }
                set
                {
                    this.cj_prijemceField = value;
                }
            }

            /// <remarks/>
            public ResponseSubjektPodporaRezim_podpory rezim_podpory
            {
                get
                {
                    return this.rezim_podporyField;
                }
                set
                {
                    this.rezim_podporyField = value;
                }
            }

            /// <remarks/>
            public int id_podpory
            {
                get
                {
                    return this.id_podporyField;
                }
                set
                {
                    this.id_podporyField = value;
                }
            }

            /// <remarks/>
            public int poskytovatel_id
            {
                get
                {
                    return this.poskytovatel_idField;
                }
                set
                {
                    this.poskytovatel_idField = value;
                }
            }

            /// <remarks/>
            public string poskytovatel_ojm
            {
                get
                {
                    return this.poskytovatel_ojmField;
                }
                set
                {
                    this.poskytovatel_ojmField = value;
                }
            }

            /// <remarks/>
            public int poskytovatel_ic
            {
                get
                {
                    return this.poskytovatel_icField;
                }
                set
                {
                    this.poskytovatel_icField = value;
                }
            }

            /// <remarks/>
            public int stav
            {
                get
                {
                    return this.stavField;
                }
                set
                {
                    this.stavField = value;
                }
            }

            /// <remarks/>
            public ResponseSubjektPodporaSeznam_priznaky seznam_priznaky
            {
                get
                {
                    return this.seznam_priznakyField;
                }
                set
                {
                    this.seznam_priznakyField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
            public System.DateTime insdat
            {
                get
                {
                    return this.insdatField;
                }
                set
                {
                    this.insdatField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
            public System.DateTime edidat
            {
                get
                {
                    return this.edidatField;
                }
                set
                {
                    this.edidatField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlIgnoreAttribute()]
            public bool edidatSpecified
            {
                get
                {
                    return this.edidatFieldSpecified;
                }
                set
                {
                    this.edidatFieldSpecified = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.pds.eu/RDM_PUB01B")]
        public partial class ResponseSubjektPodporaRezim_podpory
        {

        private bool adhocField;
        
        private string nazevField;
        
        private System.DateTime datum_odField;
        
        private bool datum_odFieldSpecified;
        
        private System.DateTime datum_doField;
        
        private bool datum_doFieldSpecified;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
        public bool adhoc
        {
            get
            {
                return this.adhocField;
            }
            set
            {
                this.adhocField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute()]
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
        [System.Xml.Serialization.XmlElementAttribute(DataType="date")]
        public System.DateTime datum_od
        {
            get
            {
                return this.datum_odField;
            }
            set
            {
                this.datum_odField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool datum_odSpecified
        {
            get
            {
                return this.datum_odFieldSpecified;
            }
            set
            {
                this.datum_odFieldSpecified = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType="date")]
        public System.DateTime datum_do
        {
            get
            {
                return this.datum_doField;
            }
            set
            {
                this.datum_doField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool datum_doSpecified
        {
            get
            {
                return this.datum_doFieldSpecified;
            }
            set
            {
                this.datum_doFieldSpecified = value;
            }
        }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.pds.eu/RDM_PUB01B")]
        public partial class ResponseSubjektPodporaSeznam_priznaky
        {

            private ResponseSubjektPodporaSeznam_priznakyPriznak priznakField;

            /// <remarks/>
            public ResponseSubjektPodporaSeznam_priznakyPriznak priznak
            {
                get
                {
                    return this.priznakField;
                }
                set
                {
                    this.priznakField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.pds.eu/RDM_PUB01B")]
        public partial class ResponseSubjektPodporaSeznam_priznakyPriznak
        {

            private int kodField;

            private string nazevField;

            private string popisField;

            /// <remarks/>
            public int kod
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
            public string popis
            {
                get
                {
                    return this.popisField;
                }
                set
                {
                    this.popisField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.pds.eu/RDM_PUB01B")]
        public partial class ResponseSubjektUcetni_obdobi
        {

            private System.DateTime datum_doField;

            private System.DateTime datum_odField;

            private int id_uoField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
            public System.DateTime datum_do
            {
                get
                {
                    return this.datum_doField;
                }
                set
                {
                    this.datum_doField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
            public System.DateTime datum_od
            {
                get
                {
                    return this.datum_odField;
                }
                set
                {
                    this.datum_odField = value;
                }
            }

            /// <remarks/>
            public int id_uo
            {
                get
                {
                    return this.id_uoField;
                }
                set
                {
                    this.id_uoField = value;
                }
            }
        }




    }
}