using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArangoDB.Client;

namespace IMDb_local_datamodel {
    [CollectionProperty(Naming = NamingConvention.ToCamelCase, CollectionName = "title_ratings")]

    public class Title_ratings:ITSV {
        //tconst(string) - alphanumeric unique identifier of the title
        [DocumentProperty(Identifier = IdentifierType.Key)]
        public string _key { get; set; }
        //averageRating – weighted average of all the individual user ratings
        public double averageRating { get; set; }
        //numVotes - number of votes the title has received
        public long numVotes { get; set; }

        public void Initialize(string line) {
            string[] values = line.Split('\t');
            try {
                _key = values[0];
            } catch { }
            try {
                if (values[1] != "\\N") {
                    averageRating = double.Parse(values[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                } else {
                    averageRating = -1;
                }
            } catch { }

            try {
                if (values[2] != "\\N") {
                    numVotes = long.Parse(values[2]);
                } else {
                    numVotes = -1;
                }
            } catch { }
        }
    }
}
