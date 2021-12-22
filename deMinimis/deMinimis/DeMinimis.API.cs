using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace deMinimis
{
    public static class DeMinimisCalls
    {


        static string subjIdSubReq = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:v02=""http://www.pds.eu/vOKO/v0200"" xmlns:rdm=""http://www.pds.eu/RDM_PUB01B"" xmlns:ns=""http://www.mze.cz/ESBServer/1.0"">
<soapenv:Header/>
<soapenv:Body>
<v02:Request vOKOid=""RDM_PUB01B"">
<v02:RequestContent>
<rdm:Request>
<rdm:dotaz_detail>
    <rdm:dotaz_subjektid>
        <rdm:subjektid>#SUBJID#</rdm:subjektid>
    </rdm:dotaz_subjektid>
</rdm:dotaz_detail>
</rdm:Request>
</v02:RequestContent>
</v02:Request>
</soapenv:Body>
</soapenv:Envelope>
";

        static string icoSubReq = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:v02=""http://www.pds.eu/vOKO/v0200"" xmlns:rdm=""http://www.pds.eu/RDM_PUB01B"" xmlns:ns=""http://www.mze.cz/ESBServer/1.0"">
<soapenv:Header/>
<soapenv:Body>
<v02:Request vOKOid=""RDM_PUB01B"">
<v02:RequestContent>
<rdm:Request>
<rdm:dotaz_detail>
    <rdm:dotaz_ic>
        <rdm:ic>#ICO#</rdm:ic>
    </rdm:dotaz_ic>
</rdm:dotaz_detail>
</rdm:Request>
</v02:RequestContent>
</v02:Request>
</soapenv:Body>
</soapenv:Envelope>
";

        //DATUMOD 2011-05-09
        static string icoChangesReq = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:v02=""http://www.pds.eu/vOKO/v0200"" xmlns:rdm=""http://www.pds.eu/RDM_PUB01B"" xmlns:ns=""http://www.mze.cz/ESBServer/1.0"">
<soapenv:Header/>
<soapenv:Body>
<v02:Request vOKOid=""RDM_PUB01B"">
<v02:RequestContent>
<rdm:Request>
<rdm:dotaz_zmeny>
    <rdm:zmeny_od>#DATUMOD#</rdm:zmeny_od>
</rdm:dotaz_zmeny>
</rdm:Request>
</v02:RequestContent>
</v02:Request>
</soapenv:Body>
</soapenv:Envelope>
";


        /*
Hodnoty:
- c_forma_podpory
- c_priznaky_podpory
- c_stav_podpory
- c_duvod_zruseni_podpory
- c_pravni_akt_poskytnuti
- d_rdm_oblast
- d_rdm_narizeni*/
         public static deMinimis.Response.Ciselnik[] Ciselnik(string ciselnik)
        {

            string url = "https://eagri.cz/ssl/nosso-app/EPO/WS/v2Online/vOKOsrv.ashx";
            string req = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:v02=""http://www.pds.eu/vOKO/v0200"" xmlns:rdm=""http://www.pds.eu/RdmServices/RDM_CIS01A"">
   <soapenv:Header/>
   <soapenv:Body>
      <v02:Request vOKOid=""RDM_CIS01A"">
         <v02:RequestContent>
            <rdm:Request>
               <rdm:ciselnik_nazev>#CISELNIK#</rdm:ciselnik_nazev>
            </rdm:Request>
         </v02:RequestContent>
      </v02:Request>
   </soapenv:Body>
</soapenv:Envelope>".Replace("#CISELNIK#", ciselnik);

            Soap net = new Soap();
            string resp = net.UploadString(url, req);

            XElement xdoc = XElement.Parse(resp);
            var res = xdoc.DescendantNodes()
                .Select(m => m as XElement)
                .Where(m => m != null && m.HasAttributes)
                .Where(m => m.Attributes()
                        .Any(a => a.Name == "xmlns" && a.Value == "http://www.pds.eu/RdmServices/RDM_CIS01A") == true
                    )
                .Elements()
                .FirstOrDefault(m => m.Name.LocalName == "ciselnik_zaznamy")
                .Elements().Where(m=>m.Name.LocalName=="zaznam")
                .Select(m=> new deMinimis.Response.Ciselnik()
                                {
                                    kod = int.Parse(m.Element("{http://www.pds.eu/RdmServices/RDM_CIS01A}kod").Value),
                                    nazev = m.Element("{http://www.pds.eu/RdmServices/RDM_CIS01A}nazev").Value,
                                    popis = m.Element("{http://www.pds.eu/RdmServices/RDM_CIS01A}popis")?.Value
                                }
                    )
                .ToArray();
                    

             return res;

        }

        public static deMinimis.Response.Detail.Response GetSubjPerSubjectId(string subjectId)
        {

            string url = "https://eagri.cz/ssl/nosso-app/EPO/WS/v2Online/vOKOsrv.ashx";
            string req = subjIdSubReq.Replace("#SUBJID#", subjectId);

            Soap net = new Soap();
            string resp = net.UploadString(url, req);

            XElement xdoc = XElement.Parse(resp);
            var res = xdoc.DescendantNodes()
                .Select(m => m as XElement)
                .Where(m => m != null && m.HasAttributes)
                .Where(m => m.Attributes()
                        .Any(a => a.Name == "xmlns" && a.Value == "http://www.pds.eu/RDM_PUB01B") == true
                    )
                    .ToArray();
            var serializer = new XmlSerializer(typeof(deMinimis.Response.Detail.Response), "http://www.pds.eu/RDM_PUB01B");
            deMinimis.Response.Detail.Response resObj = null;
            using (var read = res.First().CreateReader())
            {
                resObj = (deMinimis.Response.Detail.Response)serializer.Deserialize(res.First().CreateReader());
            }
             return resObj;

        }
        public static deMinimis.Response.Detail.Response GetSubjPerIco(string ico)
        {

            string url = "https://eagri.cz/ssl/nosso-app/EPO/WS/v2Online/vOKOsrv.ashx";
            string req = icoSubReq.Replace("#ICO#", ico);

            Soap net = new Soap();
            string resp = net.UploadString(url, req);

            XElement xdoc = XElement.Parse(resp);
            var res = xdoc.DescendantNodes()
                .Select(m => m as XElement)
                .Where(m => m != null && m.HasAttributes)
                .Where(m => m.Attributes()
                        .Any(a => a.Name == "xmlns" && a.Value == "http://www.pds.eu/RDM_PUB01B") == true
                    )
                    .ToArray();
            var serializer = new XmlSerializer(typeof(deMinimis.Response.Detail.Response), "http://www.pds.eu/RDM_PUB01B");
            deMinimis.Response.Detail.Response resObj = null;
            using (var read = res.First().CreateReader())
            {
                resObj = (deMinimis.Response.Detail.Response)serializer.Deserialize(res.First().CreateReader());
            }
            return resObj;

        }
           public static deMinimis.Response.Changes.Response GetChanges(DateTime odDatum)
        {

            string url = "https://eagri.cz/ssl/nosso-app/EPO/WS/v2Online/vOKOsrv.ashx";
            string req = icoChangesReq.Replace("#DATUMOD#", odDatum.ToString("yyyy-MM-dd"));

            Soap net = new Soap();
            string resp = net.UploadString(url, req);

            XElement xdoc = XElement.Parse(resp);
            var res = xdoc.DescendantNodes()
                .Select(m => m as XElement)
                .Where(m => m != null && m.HasAttributes)
                .Where(m => m.Attributes()
                        .Any(a => a.Name == "xmlns" && a.Value == "http://www.pds.eu/RDM_PUB01B") == true
                    )
                    .ToArray();
            var serializer = new XmlSerializer(typeof(deMinimis.Response.Changes.Response), "http://www.pds.eu/RDM_PUB01B");
            deMinimis.Response.Changes.Response resObj = null;
            using (var read = res.First().CreateReader())
            {
                resObj = (deMinimis.Response.Changes.Response)serializer.Deserialize(res.First().CreateReader());
            }
            return resObj;

        }

        class Soap : System.Net.WebClient
        {
            public int Timeout { get; set; }
            public Soap() : this(60 * 1000) { } //1min

            public Soap(int timeout)
            {
                this.Timeout = timeout;
                this.Encoding = System.Text.Encoding.UTF8;
                //this.Proxy = new System.Net.WebProxy("127.0.0.1", 8888);
            }

            protected override WebRequest GetWebRequest(Uri address)
            {

                var request = base.GetWebRequest(address);
                request.ContentType = "text/xml";
                if (request != null)
                {
                    ((HttpWebRequest)request).KeepAlive = false;
                    ((HttpWebRequest)request).ReadWriteTimeout = Timeout;
                    request.Timeout = this.Timeout;
                }
                return request;
            }

        }

    }
}
