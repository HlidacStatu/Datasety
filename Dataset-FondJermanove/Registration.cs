using System;

namespace FondJermanove
{




    public class Registration
    {
        public class Template
        {
            public string header { get; set; } = null;
            public string body { get; set; } = null;
            public string footer { get; set; } = null;
            public string title { get; set; } = null;
            public string[] properties { get; set; } = null;
        }

        public string id { get { return datasetId; } }
        public string name { get; set; }
        public string datasetId { get; set; }
        public string origUrl { get; set; }
        public string sourcecodeUrl { get; set; }
        public string description { get; set; }
        public Newtonsoft.Json.Schema.JSchema jsonSchema { get; set; }

        public bool betaversion { get; set; } = false;
        public bool allowWriteAccess { get; set; } = false;

        public Template searchResultTemplate { get; set; }
        public Template detailTemplate { get; set; }

        public string[,] orderList { get; set; } = null;


    }

}
