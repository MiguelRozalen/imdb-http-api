using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArangoDB.Client;
using Newtonsoft.Json;

namespace IMDb_local_datamodel {
    [CollectionProperty(Naming = NamingConvention.ToCamelCase, CollectionName = "title_principals")]
    public class Title_principals:ITSV {
        //tconst(string) - alphanumeric unique identifier of the title
        [DocumentProperty(Identifier = IdentifierType.Key)]
        public string _key { get; set; }

        public string titleKey { get; set; }

        public int ordering { get; set; }

        public string actorKey { get; set; }

        public string category { get; set; }

        public string job { get; set; }

        public List<string> characters { get; set; }


        public void Initialize(string line) {
            string[] values = line.Split('\t');
            try {
                titleKey = values[0];
            } catch { }
            try {
                if (values[1] != "\\N") {
                    ordering = int.Parse(values[1]);
                } else {
                    ordering = -1;
                }
            } catch { }

            try {
                if (values[2] != "\\N") {
                    actorKey = values[2];
                }
            } catch { }

            try {
                if (values[3] != "\\N") {
                    category = values[3];
                }
            } catch { }

            try {
                if (values[4] != "\\N") {
                    job = values[4];
                }
            } catch { }
            try {
                characters = new List<string>();
                if (values[5] != "\\N") {
                    characters.AddRange(JsonConvert.DeserializeObject<List<string>>(values[5]));
                    //characters.Add(values[5]);
                }
            } catch(Exception ex) {
                Console.WriteLine(ex);
            }
        }
    }
}
