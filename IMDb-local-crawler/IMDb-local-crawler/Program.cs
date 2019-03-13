using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ArangoDB.Client;
using HtmlAgilityPack;
using IMDb_local_datamodel;
using Newtonsoft.Json;

namespace IMDb_local_crawler {
    class Program {

        const string URL_DATABASE = "http://datasets.imdbws.com/";

        const string BASICS_NAME_FILE = "name.basics.tsv.gz";

        const string AKAS_FILE = "title.akas.tsv.gz";

        const string BASICS_TITLE_FILE = "title.basics.tsv.gz";

        const string CREW_FILE = "title.crew.tsv.gz";

        const string EPISODES_FILE = "title.episode.tsv.gz";

        const string PRINCIPALS_FILE = "title.principals.tsv.gz";

        const string RATINGS_FILE = "title.ratings.tsv.gz";

        static void Main(string[] args) {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            const string URI = "http://localhost:8529";
            ArangoDatabase.ChangeSetting(s => {
                s.Database = "imdb";
                s.Url = URI;
                s.Credential = new NetworkCredential("root", "");
                s.SystemDatabaseCredential = new NetworkCredential("root", "");
                s.WaitForSync = true;
            });

            //Step_0_ConfigureDatabase(URI);

            //First of all, download and decompress every file
            //Step_1_IMDbCrawler();

            //Second, get all spanish titles
            //Step_2_MergeInformation();

            //Third, get all spanish names
            //Step_3_GetSpaniards();

            //Fourth, Merge and Save Information
            //Step_4_MergeSpanishInfo();

            //Fifth CrawlIMDBWebpage
            Step_5_RetrieveIMDbSocialInformation();

            //Get Non-clasiffied users


            Console.ReadLine();
        }

        private static void Step_5_RetrieveIMDbSocialInformation() {
            List<Filtered_person_es> people = new List<Filtered_person_es>();
            Random rnd = new Random();

            int remaining = 1;
            while (remaining > 0) {
                using (IArangoDatabase db = ArangoDatabase.CreateWithSetting()) {
                    people = db.CreateStatement<Filtered_person_es>("FOR r IN filtered_person_es FILTER r.twitterScreenName==null AND r.instagramScreenName==null SORT r.birthYear DESC, r.primaryName ASC LIMIT 10 RETURN r").ToList();

                    for (int i = 0; i < people.Count; i++) {
                        Filtered_person_es person = people[i];
                        Console.WriteLine("Getting IMBd social profiles information for {0}: {1}", person._key, person.primaryName);
                        CrawlSocialNetworkFromImdb(ref person);
                        db.UpdateById<Filtered_person_es>(person._key, person);
                        person = null;
                        // Sleep 0.5-0.7 sec to try avoid API LIMIT

                        Thread.Sleep(800 + 100 * rnd.Next(0, 3));
                    }
                    remaining = db.CreateStatement<int>("RETURN COUNT(FOR r IN filtered_person_es FILTER r.twitterScreenName==null AND r.instagramScreenName==null RETURN r)").ToList().FirstOrDefault();
                    Console.WriteLine("REMAINING {0}", remaining);
                    Thread.Sleep(800 + 100 * rnd.Next(0, 3));
                }
            }
        }

        private static void CrawlSocialNetworkFromImdb(ref Filtered_person_es person) {
            const string URI_IMDB_NAME = "https://www.imdb.com/name";
            const string URI_IMDB_SOCIAL = "https://www.imdb.com";
            const string UNKNOWN = "unknown";

            person.twitterScreenName = UNKNOWN;
            person.instagramScreenName = UNKNOWN;
            try {
                // Create web request to obtain HTML.
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format("{0}/{1}", URI_IMDB_NAME, person._key));
                request.AllowAutoRedirect = true;
                request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36 ";

                // With response.
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
                    // Response OK.
                    switch (response.StatusCode) {
                        case HttpStatusCode.OK:
                            // Obtain HTML.
                            Stream receiveStream = response.GetResponseStream();
                            StreamReader readStream = null;
                            if (response.CharacterSet == null) {
                                readStream = new StreamReader(receiveStream);
                            } else {
                                readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                            }
                            // With HTML.
                            using (readStream) {
                                // Load HTML.
                                HtmlDocument htmlDocument = new HtmlDocument();
                                string html = string.Empty;
                                html = readStream.ReadToEnd();
                                htmlDocument.LoadHtml(html);

                                List<HtmlNode> links = htmlDocument?.DocumentNode?.Descendants("div")?.Where(p => p.Id == "details-official-sites")?.FirstOrDefault()?.Descendants("a")?.ToList();
                                if (links != null) {
                                    for (int i = 0; i < links.Count; i++) {
                                        if (links[i].OuterHtml.ToLower().Contains("twitter")) {
                                            string link = links[i].GetAttributeValue("href", "");
                                            if (!string.IsNullOrEmpty(link)) {
                                                //NAVIGATE TWITTER
                                                HttpWebRequest request2 = (HttpWebRequest)WebRequest.Create(string.Format("{0}{1}", URI_IMDB_SOCIAL, link));
                                                request2.AllowAutoRedirect = true;
                                                request2.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                                                request2.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36 ";
                                                try {
                                                    using (HttpWebResponse response2 = (HttpWebResponse)request2.GetResponse()) {
                                                        person.twitterScreenName = response2.ResponseUri.ToString().Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Last();
                                                    }
                                                } catch (Exception ex) {

                                                }
                                            }
                                        }
                                        if (links[i].OuterHtml.ToLower().Contains("instagram")) {
                                            string link = links[i].GetAttributeValue("href", "");
                                            if (!string.IsNullOrEmpty(link)) {
                                                //NAVIGATE TWITTER
                                                HttpWebRequest request2 = (HttpWebRequest)WebRequest.Create(string.Format("{0}{1}", URI_IMDB_SOCIAL, link));
                                                request2.AllowAutoRedirect = true;
                                                request2.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                                                request2.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36 ";
                                                try {
                                                    using (HttpWebResponse response2 = (HttpWebResponse)request2.GetResponse()) {
                                                        person.instagramScreenName = response2.ResponseUri.ToString().Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Last();
                                                    }
                                                } catch (Exception ex) {

                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case HttpStatusCode.Moved:
                        default:
                            break;
                    }
                }
            } catch (Exception ex) {
                Debug.Print("ERROR Getting social profiles for {0}: {1}", person._key, person.primaryName);

                if (person.twitterScreenName == UNKNOWN) {
                    person.twitterScreenName = null;
                }
                if (person.instagramScreenName == UNKNOWN) {
                    person.instagramScreenName = null;
                }

                //POSIBLE API LIMIT: Duermo 15 min
                Thread.Sleep(15 * 60 * 1000);

            }
        }

        private static void Step_4_MergeSpanishInfo() {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            List<Title_complete_es> aux = new List<Title_complete_es>();
            using (IArangoDatabase db = ArangoDatabase.CreateWithSetting()) {
                //Get all real spanish
                aux = db.CreateStatement<Title_complete_es>("FOR r IN title_complete_es FILTER r.basics.originalTitle==r.akas.title RETURN DISTINCT r").ToList();
            }

            if (aux.Count > 0) {
                List<Filtered_title_es> titlesToInsert = new List<Filtered_title_es>();
                for (int i = 0; i < aux.Count; i++) {
                    Filtered_title_es name = new Filtered_title_es();
                    name._key = aux[i]._key;
                    name.title = aux[i].basics.originalTitle;
                    name.titleType = aux[i].basics.titleType;
                    name.startYear = aux[i].basics.startYear;
                    name.endYear = aux[i].basics.endYear;
                    name.runtimeMinutes = aux[i].basics.runtimeMinutes;
                    name.genres = aux[i].basics.genres;

                    name.directors = new List<string>();
                    for (int j = 0; j < aux[i].crew.directors.Count; j++) {
                        using (IArangoDatabase db = ArangoDatabase.CreateWithSetting()) {
                            Name_es director = db.CreateStatement<Name_es>("FOR r IN name_basics_es FILTER r._key=='" + aux[i].crew.directors[j] + "' RETURN DISTINCT r").ToList().FirstOrDefault();
                            if (director != null) {
                                Filtered_person_es dir = new Filtered_person_es();
                                dir._key = director._key;
                                dir.birthYear = director.birthYear;
                                dir.deathYear = director.deathYear;
                                dir.primaryName = director.primaryName;
                                dir.primaryProfession = director.primaryProfession;
                                if (!db.Exists<Filtered_person_es>(dir._key)) {
                                    db.Insert<Filtered_person_es>(dir);
                                }

                                name.directors.Add(dir._key);
                            }
                        }
                    }

                    name.writers = new List<string>();
                    for (int j = 0; j < aux[i].crew.writers.Count; j++) {
                        using (IArangoDatabase db = ArangoDatabase.CreateWithSetting()) {
                            Name_es writer = db.CreateStatement<Name_es>("FOR r IN name_basics_es FILTER r._key=='" + aux[i].crew.writers[j] + "' RETURN DISTINCT r").ToList().FirstOrDefault();
                            if (writer != null) {
                                Filtered_person_es wri = new Filtered_person_es();
                                wri._key = writer._key;
                                wri.birthYear = writer.birthYear;
                                wri.deathYear = writer.deathYear;
                                wri.primaryName = writer.primaryName;
                                wri.primaryProfession = writer.primaryProfession;

                                if (!db.Exists<Filtered_person_es>(wri._key)) {
                                    db.Insert<Filtered_person_es>(wri);
                                }

                                name.writers.Add(wri._key);
                            }
                        }
                    }

                    name.starring = new List<string>();
                    for (int j = 0; j < aux[i].principals.Count; j++) {
                        using (IArangoDatabase db = ArangoDatabase.CreateWithSetting()) {
                            Name_es actr = db.CreateStatement<Name_es>("FOR r IN name_basics_es FILTER r._key=='" + aux[i].principals[j].actorKey + "' RETURN DISTINCT r").ToList().FirstOrDefault();
                            if (actr != null) {
                                Filtered_person_es act = new Filtered_person_es();
                                act._key = actr._key;
                                act.birthYear = actr.birthYear;
                                act.deathYear = actr.deathYear;
                                act.primaryName = actr.primaryName;
                                act.primaryProfession = actr.primaryProfession;

                                if (!db.Exists<Filtered_person_es>(act._key)) {
                                    db.Insert<Filtered_person_es>(act);
                                }

                                name.starring.Add(act._key);
                            }
                        }
                    }

                    name.episodes = new List<Filtered_episode>();
                    for (int j = 0; j < aux[i].episodes.Count; j++) {
                        Filtered_episode episode = new Filtered_episode();
                        episode.episodeNumber = aux[i].episodes[j].episodeNumber;
                        episode.seasonNumber = aux[i].episodes[j].seasonNumber;
                        name.episodes.Add(episode);
                    }

                    if (aux[i]?.ratings?.FirstOrDefault() != null) {
                        name.rating = new Tuple<double, long>(aux[i].ratings.FirstOrDefault().averageRating, aux[i].ratings.FirstOrDefault().numVotes);
                    } else {
                        name.rating = new Tuple<double, long>(0, 0);
                    }

                    titlesToInsert.Add(name);

                    if (titlesToInsert.Count > 100) {
                        using (IArangoDatabase db = ArangoDatabase.CreateWithSetting()) {
                            db.InsertMultiple<Filtered_title_es>(titlesToInsert);
                        }
                        titlesToInsert.Clear();
                        Console.WriteLine("Retreaving information for names... Total:{0}%", ((double)i * 100 / aux.Count).ToString("0.00"));
                    }
                }

                if (titlesToInsert.Count > 0) {
                    using (IArangoDatabase db = ArangoDatabase.CreateWithSetting()) {
                        db.InsertMultiple<Filtered_title_es>(titlesToInsert);
                    }
                    titlesToInsert.Clear();
                    Console.WriteLine("Retreaving information for names... Total:100%");
                }
            }
            watch.Stop();
            Console.WriteLine("\nTotal Time to merge spanish filtered entities: {0} seconds", watch.Elapsed.TotalSeconds.ToString());
        }

        private static void Step_0_ConfigureDatabase(string URI) {
            using (IArangoDatabase db = ArangoDatabase.CreateWithSetting()) {
                try {
                    db.CreateCollection("name_basics");
                } catch { }
                SetIndex("root", "", URI, "imdb", "name_basics", new string[] { "primaryName" });
                try {
                    db.CreateCollection("name_basics_es");
                } catch { }
                SetIndex("root", "", URI, "imdb", "name_basics_es", new string[] { "primaryName" });
                try {
                    db.CreateCollection("title_akas");
                } catch { }
                SetIndex("root", "", URI, "imdb", "title_akas", new string[] { "region" });
                SetIndex("root", "", URI, "imdb", "title_akas", new string[] { "title" });
                SetIndex("root", "", URI, "imdb", "title_akas", new string[] { "titleId" });

                try {
                    db.CreateCollection("title_basics");
                } catch { }
                SetIndex("root", "", URI, "imdb", "title_basics", new string[] { "primaryTitle" });
                SetIndex("root", "", URI, "imdb", "title_basics", new string[] { "originalTitle" });
                try {
                    db.CreateCollection("title_complete_es");
                } catch { }
                try {
                    db.CreateCollection("title_crew");
                } catch { }
                try {
                    db.CreateCollection("title_episode");
                } catch { }
                SetIndex("root", "", URI, "imdb", "title_episode", new string[] { "parentTconst" });
                try {
                    db.CreateCollection("title_principals");
                } catch { }
                SetIndex("root", "", URI, "imdb", "title_principals", new string[] { "actorKey" });
                SetIndex("root", "", URI, "imdb", "title_principals", new string[] { "titleKey" });
                try {
                    db.CreateCollection("title_ratings");
                } catch { }

                try {
                    db.CreateCollection("title_complete_es");
                } catch { }
                try {
                    db.CreateCollection("filtered_title_es");
                } catch { }
                try {
                    db.CreateCollection("filtered_person_es");
                } catch { }
            }
        }

        private static void Step_3_GetSpaniards() {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            //Get all writters and directors
            List<string> spanish_ids = new List<string>();
            List<Name_es> spanish_names = new List<Name_es>();
            using (IArangoDatabase db = ArangoDatabase.CreateWithSetting()) {
                if (db != null) {
                    List<string> aux = db.CreateStatement<string>("FOR r IN title_complete_es FILTER r.basics.originalTitle==r.akas.title FILTER COUNT(r.crew.directors>0) FOR d in r.crew.directors FILTER d != '\\N' RETURN DISTINCT d").ToList();
                    if (aux.Count > 0) {
                        spanish_ids.AddRange(aux);
                    }
                    aux = db.CreateStatement<string>("FOR r IN title_complete_es FILTER r.basics.originalTitle==r.akas.title FILTER COUNT(r.crew.writers>0) FOR d in r.crew.directors FILTER d != '\\N' RETURN DISTINCT d").ToList();
                    if (aux.Count > 0) {
                        spanish_ids.AddRange(aux);
                    }
                    aux = db.CreateStatement<string>("FOR r IN title_complete_es FILTER r.basics.originalTitle==r.akas.title FILTER COUNT(r.principals>0) FOR d in r.principals FILTER d.actorKey != '\\N' RETURN DISTINCT d.actorKey").ToList();
                    if (aux.Count > 0) {
                        spanish_ids.AddRange(aux);
                    }
                }
            }

            for (int i = 0; i < spanish_ids.Count; i++) {
                try {
                    using (IArangoDatabase db = ArangoDatabase.CreateWithSetting()) {

                        Name_es name = db.CreateStatement<Name_es>("FOR r IN name_basics FILTER r._key=='" + spanish_ids[i] + "' RETURN r").ToList().FirstOrDefault();
                        if (name != null) {
                            spanish_names.Add(name);
                        }

                        if (spanish_names.Count > 100) {
                            db.InsertMultiple<Name_es>(spanish_names);
                            spanish_names.Clear();
                            Console.WriteLine("Retreaving information for names... Total:{0}%", ((double)i * 100 / spanish_ids.Count).ToString("0.00"));
                        }
                    }
                } catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                }
            }
            if (spanish_names.Count > 0) {
                using (IArangoDatabase db = ArangoDatabase.CreateWithSetting()) {
                    db.InsertMultiple<Name_es>(spanish_names);
                    Console.WriteLine("Retreaving information for names... Total:100.00%");
                }
            }

            watch.Stop();
            Console.WriteLine("\nTotal Time to retrieve spanish names: {0} seconds", watch.Elapsed.TotalSeconds.ToString());
        }

        private static void Step_2_MergeInformation() {
            //Second, get all spanish entities
            Stopwatch watch = new Stopwatch();
            watch.Start();
            List<Title_akas> spanish_entities = new List<Title_akas>();
            using (IArangoDatabase db = ArangoDatabase.CreateWithSetting()) {
                if (db != null) {
                    spanish_entities = db.CreateStatement<Title_akas>("FOR r IN title_akas FILTER r.region == 'ES' RETURN DISTINCT r").ToList();
                }
            }

            List<Title_complete_es> title_complete_es = new List<Title_complete_es>();
            for (int i = 0; i < spanish_entities.Count; i++) {
                try {
                    using (IArangoDatabase db = ArangoDatabase.CreateWithSetting()) {
                        //Third, get all title complete
                        Title_complete_es complete = new Title_complete_es();
                        complete._key = spanish_entities[i].titleId;
                        complete.akas = spanish_entities[i];
                        try {
                            complete.basics = db.CreateStatement<Title_basics>("FOR r IN title_basics FILTER r._key == '" + complete._key + "' RETURN r").ToList().First();
                        } catch { }
                        if (!complete.basics.isAdult) {
                            try {
                                complete.crew = db.CreateStatement<Title_crew>("FOR r IN title_crew FILTER r._key == '" + complete._key + "' RETURN r").ToList().First();
                            } catch { }

                            try {
                                complete.episodes = db.CreateStatement<Title_episode>("FOR r IN title_episode FILTER r.parentTconst == '" + complete._key + "' RETURN r").ToList();
                            } catch { }

                            try {
                                complete.principals = db.CreateStatement<Title_principals>("FOR r IN title_principals FILTER r.titleKey == '" + complete._key + "' RETURN r").ToList();
                            } catch { }

                            try {
                                complete.ratings = db.CreateStatement<Title_ratings>("FOR r IN title_ratings FILTER r._key == '" + complete._key + "' RETURN r").ToList();
                            } catch { }

                            title_complete_es.Add(complete);
                        }
                        if (title_complete_es.Count > 100) {
                            db.InsertMultiple<Title_complete_es>(title_complete_es);
                            title_complete_es.Clear();
                            Console.WriteLine("Retreaving information for title... Total:{0}%", ((double)i * 100 / spanish_entities.Count).ToString("0.00"));
                        }
                    }
                } catch (Exception ex) {
                    Console.WriteLine(ex);
                }
            }
            if (title_complete_es.Count > 0) {
                using (IArangoDatabase db = ArangoDatabase.CreateWithSetting()) {
                    db.InsertMultiple<Title_complete_es>(title_complete_es);
                    Console.WriteLine("Retreaving information for title... Total:100.00%");
                }
            }
            watch.Stop();
            Console.WriteLine("\nTotal Time to merge spanish titles: {0} seconds", watch.Elapsed.TotalSeconds.ToString());
        }

        private static void Step_1_IMDbCrawler() {
            using (var client = new WebClient()) {

                Stopwatch watch = new Stopwatch();
                watch.Start();
                ParseIMDbFile<Name_basics>(client, BASICS_NAME_FILE);
                watch.Stop();
                Console.WriteLine("\nTotal Time to consolidate Name_basics: {0} seconds", watch.Elapsed.TotalSeconds.ToString());

                watch = new Stopwatch();
                watch.Start();
                ParseIMDbFile<Title_akas>(client, AKAS_FILE);
                watch.Stop();
                Console.WriteLine("Total Time to consolidate Title_akas: {0} seconds", watch.Elapsed.TotalSeconds.ToString());

                watch = new Stopwatch();
                watch.Start();
                ParseIMDbFile<Title_basics>(client, BASICS_TITLE_FILE);
                watch.Stop();
                Console.WriteLine("\nTotal Time to consolidate Title_basics: {0} seconds", watch.Elapsed.TotalSeconds.ToString());

                watch = new Stopwatch();
                watch.Start();
                ParseIMDbFile<Title_crew>(client, CREW_FILE);
                watch.Stop();
                Console.WriteLine("\nTotal Time to consolidate Title_crew: {0} seconds", watch.Elapsed.TotalSeconds.ToString());

                watch = new Stopwatch();
                watch.Start();
                ParseIMDbFile<Title_episode>(client, EPISODES_FILE);
                watch.Stop();
                Console.WriteLine("\nTotal Time to consolidate Title_episode: {0} seconds", watch.Elapsed.TotalSeconds.ToString());

                watch = new Stopwatch();
                watch.Start();
                ParseIMDbFile<Title_principals>(client, PRINCIPALS_FILE);
                watch.Stop();
                Console.WriteLine("\nTotal Time to consolidate Title_principals: {0} seconds", watch.Elapsed.TotalSeconds.ToString("0.00"));

                watch = new Stopwatch();
                watch.Start();
                ParseIMDbFile<Title_ratings>(client, RATINGS_FILE);
                watch.Stop();
                Console.WriteLine("\nTotal Time to consolidate Title_ratings: {0} seconds", watch.Elapsed.TotalSeconds.ToString("0.00"));
            }
        }

        private static void ParseIMDbFile<T>(WebClient client, string NAME_FILE) where T : ITSV, new() {
            string fileName;
            Console.WriteLine("Downloading file {0}...", NAME_FILE);
            client.DownloadFile(string.Concat(URL_DATABASE, NAME_FILE), NAME_FILE);
            Console.WriteLine("Downloading file {0}... DONE!", NAME_FILE);

            Console.WriteLine("Decompressing file {0}...", NAME_FILE);
            fileName = Utils.Decompress(new FileInfo(NAME_FILE));
            Console.WriteLine("Decompressing file {0}... DONE!", NAME_FILE);

            List<T> bulk_insert = new List<T>();
            int counter = 0, linesCount = 0;
            List<string> lines;
            try {

                //We are going to try read all file, if we cant we read line by line
                lines = File.ReadLines(fileName).ToList();
                linesCount = lines.Count;

                for (int i = 1; i < linesCount; i++) {
                    T record = new T();
                    record.Initialize(lines[i]);
                    bulk_insert.Add(record);

                    if (bulk_insert.Count > 1000) {
                        using (IArangoDatabase db = ArangoDatabase.CreateWithSetting()) {
                            db.InsertMultiple<T>(bulk_insert);
                        }
                        counter += bulk_insert.Count;
                        Console.WriteLine("Creating records... Total:{0}%", ((double)counter * 100 / linesCount).ToString("0.00"));
                        bulk_insert.Clear();
                    }
                }
                if (bulk_insert.Count > 0) {
                    using (IArangoDatabase db = ArangoDatabase.CreateWithSetting()) {
                        db.InsertMultiple<T>(bulk_insert);
                    }
                }
            } catch (OutOfMemoryException ex) {

                using (StreamReader file = new StreamReader(fileName)) {
                    string line;
                    while ((line = file.ReadLine()) != null) {
                        linesCount++;
                    }
                }

                using (StreamReader file = new StreamReader(fileName)) {
                    string line = file.ReadLine();
                    while ((line = file.ReadLine()) != null) {
                        T record = new T();
                        record.Initialize(line);
                        bulk_insert.Add(record);

                        if (bulk_insert.Count > 1000) {
                            using (IArangoDatabase db = ArangoDatabase.CreateWithSetting()) {
                                db.InsertMultiple<T>(bulk_insert);
                            }
                            counter += bulk_insert.Count;
                            Console.WriteLine("Creating records... Total:{0}%", ((double)counter * 100 / linesCount).ToString("0.00"));
                            bulk_insert.Clear();
                        }
                    }
                    if (bulk_insert.Count > 0) {
                        using (IArangoDatabase db = ArangoDatabase.CreateWithSetting()) {
                            db.InsertMultiple<T>(bulk_insert);
                        }
                    }
                }

            }

            Console.WriteLine("Deleting file {0}...", NAME_FILE);
            File.Delete(NAME_FILE);
            Console.WriteLine("Deleting file {0}... DONE!", NAME_FILE);

            Console.WriteLine("Deleting file {0}...", fileName);
            File.Delete(fileName);
            Console.WriteLine("Deleting file {0}... DONE!", fileName);
        }

        /// <summary>
        /// Create a new index on database
        /// </summary>
        /// <param name="collection">Collection name</param>
        /// <param name="fields">Index fields</param>
        /// <returns>Execution results</returns>
        private static bool SetIndex(string user, string password, string server, string database, string collection, string[] fields) {
            // Returned variable
            bool success = false;

            try {
                // Make the index
                Index index = new Index();
                index.type = "skiplist";
                index.unique = false;
                index.fields = fields;
                // Build http post request
                string query = string.Format("{0}/_db/{1}/_api/index?collection={2}", server, database, collection);
                HttpWebRequest request = WebRequest.Create(query) as HttpWebRequest;
                string authentication = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:{1}", user, password)));
                request.Headers.Add(HttpRequestHeader.Authorization, string.Format("basic {0}", authentication));
                request.ContentType = "application/json";
                request.Method = "POST";
                // Send request
                using (StreamWriter writer = new StreamWriter(request.GetRequestStream())) {
                    writer.Write(JsonConvert.SerializeObject(index));
                    writer.Flush();
                    writer.Close();
                }
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse) {
                    if (response.StatusCode == HttpStatusCode.Created) {
                        success = true;
                    } else {
                        success = false;
                    }
                }
            } catch {
                success = false;
            }

            // Return result
            return success;
        }
    }
    public class Index {

        public string type;

        public bool unique;

        public bool sparse;

        public string[] fields;

    }

}


