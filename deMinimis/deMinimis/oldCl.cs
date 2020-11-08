using System;
using System.Collections.Generic;
using System.Text;

namespace deMinimis
{


    public class OldCl
    {
        public int Id { get; set; }
        public int Subjektid { get; set; }
        public int Ico { get; set; }
        public string RdmOblastKod { get; set; }
        public float PodporaCzk { get; set; }
        public float PodporaEur { get; set; }
        public DateTime PodporaDatum { get; set; }
        public int PodporaFormaKod { get; set; }
        public string PodporaFormaText { get; set; }
        public string PodporaUcel { get; set; }
        public string ProjektId { get; set; }
        public string IdPodporyPrijemce { get; set; }
        public int StavKod { get; set; }
        public string StavKodText { get; set; }
        public DateTime? Insdat { get; set; }
        public DateTime? Edidat { get; set; }
        public int OrgSzrid { get; set; }
        public string PoskytovatelOjm { get; set; }
        public int PravniAktPoskytnutiId { get; set; }
        public string PravniAktPoskytnutiText { get; set; }
        public object RezimPodporyId { get; set; }
        public DateTime DbCreated { get; set; }
        public string DbCreatedBy { get; set; }
    }

}
