using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArangoDB.Client;

namespace IMDb_local_datamodel {
    [CollectionProperty(Naming = NamingConvention.ToCamelCase, CollectionName = "title_akas")]
    public class Title_akas : ITSV {
        [DocumentProperty(Identifier = IdentifierType.Key)]
        public string _key { get; set; }

        //titleId(string) - a tconst, an alphanumeric unique identifier of the title
        public string titleId { get; set; }
        //ordering(integer) – a number to uniquely identify rows for a given titleId
        public int ordering { get; set; }
        //title(string) – the localized title
        public string title { get; set; }
        //region(string) - the region for this version of the title
        public string region { get; set; }
        //language(string) - the language of the title
        public string language { get; set; }
        //types(array) - Enumerated set of attributes for this alternative title.One or more of the following: "alternative", "dvd", "festival", "tv", "video", "working", "original", "imdbDisplay". New values may be added in the future without warning
        public List<string> types { get; set; }
        //attributes (array) - Additional terms to describe this alternative title, not enumerated
        public List<string> attributes { get; set; }
        //isOriginalTitle(boolean) – 0: not original title; 1: original title
        bool isOriginalTitle { get; set; }

        public void Initialize(string line) {
            string[] values = line.Split('\t');
            try {
                titleId = values[0];
            } catch { }
            try {
                if (values[1] != "\\N") {
                    ordering = int.Parse(values[1]);
                }
            } catch (Exception ex) {
                //Console.WriteLine(ex);
            }
            try {
                if (values[2] != "\\N") {
                    title = values[2];
                }
            } catch (Exception ex) {
                //Console.WriteLine(ex);
            }
            try {
                if (values[3] != "\\N") {
                    region = values[3];
                }
            } catch (Exception ex) {
                //Console.WriteLine(ex);
            }
            try {
                if (values[4] != "\\N") {
                    language = values[4];
                }
            } catch { }
            try {
                types = new List<string>();
                if (values[5] != "\\N") {
                    types.AddRange(values[5].Split(','));
                }
            } catch { }
            try {
                attributes = new List<string>();
                if (values[6] != "\\N") {
                    attributes.AddRange(values[6].Split(','));
                }
            } catch { }
            try {
                if (values[7] != "\\N") {
                    if (values[7] == "1") {
                        isOriginalTitle = true;
                    } else {
                        isOriginalTitle = false;
                    }
                }
            } catch { }
        }

    }
}
