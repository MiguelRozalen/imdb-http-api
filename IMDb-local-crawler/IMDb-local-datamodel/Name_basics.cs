using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArangoDB.Client;

namespace IMDb_local_datamodel {

    [CollectionProperty(Naming = NamingConvention.ToCamelCase, CollectionName = "name_basics")]
    public class Name_basics : ITSV {
        //nconst(string) - alphanumeric unique identifier of the name/person
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
        //knownForTitles(array of tconsts) – titles the person is known for
        public List<string> knownForTitles { get; set; }

        public void Initialize(string line) {
            string[] values = line.Split('\t');
            try {
                _key = values[0];
            } catch { }
            try {
                if (values[1] != "\\N") {
                    primaryName = values[1];
                }
            } catch (Exception ex) {
                //Console.WriteLine(ex);
            }
            try {
                if (values[2] != "\\N") {
                    birthYear = int.Parse(values[2]);
                } else {
                    birthYear = -1;
                }
            } catch (Exception ex) {
                //Console.WriteLine(ex);
                birthYear = -1;
            }
            try {
                if (values[3] != "\\N") {
                    deathYear = int.Parse(values[3]);
                } else {
                    deathYear = -1;
                }
            } catch (Exception ex) {
                //Console.WriteLine(ex);
                deathYear = -1;
            }
            try {
                if (values[4] != "\\N") {
                    primaryProfession = new List<string>();
                    primaryProfession.AddRange(values[4].Split(','));
                }
            } catch { }
            try {
                if (values[5] != "\\N") {
                    knownForTitles = new List<string>();
                    knownForTitles.AddRange(values[5].Split(','));
                }
            } catch { }
        }

    }
}
