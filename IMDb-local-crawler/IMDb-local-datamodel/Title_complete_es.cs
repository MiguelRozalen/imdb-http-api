using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArangoDB.Client;

namespace IMDb_local_datamodel {
    [CollectionProperty(Naming = NamingConvention.ToCamelCase, CollectionName = "title_complete_es")]
    public class Title_complete_es {
        [DocumentProperty(Identifier = IdentifierType.Key)]
        public string _key { get; set; }
        public Title_akas akas;
        public Title_basics basics;
        public Title_crew crew;
        public List<Title_episode> episodes;
        public List<Title_principals> principals;
        public List<Title_ratings> ratings; 
    }
}
