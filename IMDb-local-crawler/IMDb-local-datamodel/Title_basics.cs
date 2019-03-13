using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArangoDB.Client;

namespace IMDb_local_datamodel {
    [CollectionProperty(Naming = NamingConvention.ToCamelCase, CollectionName = "title_basics")]
    public class Title_basics : ITSV {
        //tconst(string) - alphanumeric unique identifier of the title
        [DocumentProperty(Identifier = IdentifierType.Key)]
        public string _key { get; set; }

        //titleType(string) – the type/format of the title(e.g.movie, short, tvseries, tvepisode, video, etc)
        public string titleType { get; set; }

        //primaryTitle(string) – the more popular title / the title used by the filmmakers on promotional materials at the point of release
        public string primaryTitle { get; set; }

        //originalTitle(string) - original title, in the original language
        public string originalTitle { get; set; }

        //isAdult(boolean) - 0: non-adult title; 1: adult title
        public bool isAdult { get; set; }

        //startYear(YYYY) – represents the release year of a title.In the case of TV Series, it is the series start year
        public int startYear { get; set; }

        //endYear(YYYY) – TV Series end year. ‘\N’ for all other title types
        public int endYear { get; set; }

        //runtimeMinutes – primary runtime of the title, in minutes
        public int runtimeMinutes { get; set; }

        //genres(string array) – includes up to three genres associated with the title
        public List<string> genres { get; set; }

        public void Initialize(string line) {
            string[] values = line.Split('\t');
            try {
                _key = values[0];
            } catch { }
            try {
                if (values[1] != "\\N") {
                    titleType = values[1];
                }
            } catch (Exception ex) {
                //Console.WriteLine(ex);
            }
            try {
                if (values[2] != "\\N") {
                    primaryTitle = values[2];
                }
            } catch (Exception ex) {
                //Console.WriteLine(ex);
            }
            try {
                if (values[3] != "\\N") {
                    originalTitle = values[3];
                }
            } catch (Exception ex) {
                //Console.WriteLine(ex);
            }
            try {
                if (values[4] == "1") {
                    isAdult = true;
                } else {
                    isAdult = false;
                }
            } catch { }
            try {
                if (values[5] != "\\N") {
                    startYear = int.Parse(values[5]);
                } else {
                    startYear = -1;
                }
            } catch { }

            try {
                if (values[6] != "\\N") {
                    endYear = int.Parse(values[6]);
                } else {
                    endYear = -1;
                }
            } catch { }
            try {
                if (values[7] != "\\N") {
                    runtimeMinutes = int.Parse(values[7]);
                } else {
                    runtimeMinutes = -1;
                }
            } catch { }

            try {
                if (values[8] != "\\N") {
                    genres = new List<string>();
                    genres.AddRange(values[8].Split(','));
                }
            } catch { }
        }
    }
}
