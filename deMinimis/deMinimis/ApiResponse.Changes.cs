using System;
using System.Collections.Generic;
using System.Text;

namespace deMinimis.Response
{
    public class Changes
    {


        // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.pds.eu/RDM_PUB01B")]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.pds.eu/RDM_PUB01B", IsNullable = false)]
        public partial class Response
        {

            private ushort pocetField;

            private int[] seznam_subjektidField;

            /// <remarks/>
            public ushort pocet
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
            [System.Xml.Serialization.XmlArrayItemAttribute("subjektid", IsNullable = false)]
            public int[] seznam_subjektid
            {
                get
                {
                    return this.seznam_subjektidField;
                }
                set
                {
                    this.seznam_subjektidField = value;
                }
            }
        }


    }
}
