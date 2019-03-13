using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMDb_local_datamodel {
    public class Filtered_episode {
        //seasonNumber(integer) – season number the episode belongs to
        public int seasonNumber { get; set; }

        //episodeNumber(integer) – episode number of the tconst in the TV series
        public int episodeNumber { get; set; }

        public double share { get; set; }

        public long audience { get; set; }

        public string channel { get; set; }

        public string date { get; set; }

        public string sinopsis { get; set; }
    }
}
