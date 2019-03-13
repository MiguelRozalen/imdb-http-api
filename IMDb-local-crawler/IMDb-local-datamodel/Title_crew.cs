using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArangoDB.Client;

namespace IMDb_local_datamodel {
    [CollectionProperty(Naming = NamingConvention.ToCamelCase, CollectionName = "title_crew")]
    public class Title_crew :ITSV{
        //tconst(string) - alphanumeric unique identifier of the title
        [DocumentProperty(Identifier = IdentifierType.Key)]
        public string _key { get; set; }
        //directors(array of nconsts) - director(s) of the given title
        public List<string> directors { get; set; }
        //writers(array of nconsts) – writer(s) of the given title
        public List<string> writers { get; set; }

        public void Initialize(string line) {
            string[] values = line.Split('\t');
            try {
                _key = values[0];
            } catch { }
            try {
                directors = new List<string>();
                if (values[1] != "\\N") {
                    directors.AddRange(values[1].Split(','));
                }
            } catch (Exception ex) {
                //Console.WriteLine(ex);
            }
            try {
                writers = new List<string>();
                if (values[2] != "\\N") {
                    writers.AddRange(values[2].Split(','));
                }
            } catch (Exception ex) {
                //Console.WriteLine(ex);
            }
        }
    }
}
