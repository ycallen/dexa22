using Algorithms;
using Common;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace DM
{
    public class SearchResponse
    {
        public List<Candidate> Candidates { get; set; }

        public Dictionary<string, List<string>> Terms { get; set; } = new Dictionary<string, List<string>>();

        public Guid Guid { get; set; }

        public Candidate BadCandidate { get; set; }

        public bool IsESFilter { get; set; }

        public double EsElapsedTime { get; set; }

        public double JenaElapsedTime { get; set; }
    }
}
