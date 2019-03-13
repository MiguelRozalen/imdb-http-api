using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArangoDB.Client;

namespace IMDb_local_datamodel {
    [CollectionProperty(Naming = NamingConvention.ToCamelCase, CollectionName = "filtered_title_es")]
    public class Filtered_title_es {

        [DocumentProperty(Identifier = IdentifierType.Key)]
        public string _key { get; set; }

        public string title { get; set; }

        public string titleType { get; set; }

        public int startYear { get; set; }

        public int endYear { get; set; }

        public int runtimeMinutes { get; set; }

        public List<string> genres { get; set; }

        public List<string> directors { get; set; }

        public List<string> writers { get; set; }
        
        public List<string> starring { get; set; }

        public List<Filtered_episode> episodes { get; set; }

        public Tuple<double, long> rating { get; set; }
    }
}
