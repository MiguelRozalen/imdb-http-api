using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArangoDB.Client;

namespace IMDb_local_datamodel {
    [CollectionProperty(Naming = NamingConvention.ToCamelCase, CollectionName = "title_episode")]
    public class Title_episode:ITSV {
        //tconst(string) - alphanumeric identifier of episode
        [DocumentProperty(Identifier = IdentifierType.Key)]
        public string _key { get; set; }

        //parentTconst(string) - alphanumeric identifier of the parent TV Series
        public string parentTconst { get; set; }

        //seasonNumber(integer) – season number the episode belongs to
        public int seasonNumber { get; set; }

        //episodeNumber(integer) – episode number of the tconst in the TV series
        public int episodeNumber { get; set; }

        public void Initialize(string line) {
            string[] values = line.Split('\t');
            try {
                _key = values[0];
            } catch { }
            try {
                if (values[1] != "\\N") {
                    parentTconst = values[1];
                }
            } catch { }

            try {
                if (values[2] != "\\N") {
                    seasonNumber = int.Parse(values[2]);
                } else {
                    seasonNumber = -1;
                }
            } catch { }

            try {
                if (values[3] != "\\N") {
                    episodeNumber = int.Parse(values[3]);
                } else {
                    episodeNumber = -1;
                }
            } catch { }
        }
    }
}
