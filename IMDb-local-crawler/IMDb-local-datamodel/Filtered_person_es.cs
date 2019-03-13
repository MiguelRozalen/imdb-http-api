using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArangoDB.Client;

namespace IMDb_local_datamodel {

    [CollectionProperty(Naming = NamingConvention.ToCamelCase, CollectionName = "filtered_person_es")]
    public class Filtered_person_es {

        [DocumentProperty(Identifier = IdentifierType.Key)]
        public string _key { get; set; }
        //primaryName(string)– name by which the person is most often credited
        public string primaryName { get; set; }
        //birthYear – in YYYY format
        public int birthYear { get; set; }
        //deathYear – in YYYY format if applicable, else ‘\N’
        public int deathYear { get; set; }
        //primaryProfession(array of strings)– the top-3 professions of the person
        public List<string> primaryProfession { get; set; }
        public string twitterScreenName { get; set; }
        public string instagramScreenName { get; set; }

    }
}
