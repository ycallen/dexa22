using System;
using System.Collections.Generic;
using System.Text;

namespace DM
{
    public class SearchResponse
    {
        public List<Candidate> Candidates { get; set; } = new List<Candidate>();
        public Dictionary<string, List<string>> Terms { get; set; } = new Dictionary<string, List<string>>();

        public Candidate BadCandidate { get; set; }

        public bool IsESFilter { get; set; }
        
        public double EsElapsedTime { get; set; }

        public double JenaElapsedTime { get; set; }        
    }
}
