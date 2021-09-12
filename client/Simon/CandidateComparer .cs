using DM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Simon
{
    //Comparer to insert candidates into priority queue by edit distance
    public class CandidateComparer : IComparer<Candidate>
    {
        public int Compare([AllowNull] Candidate x, [AllowNull] Candidate y)
        {
            if (x.EditDistance < y.EditDistance)
                return -1;
            if (x.EditDistance > y.EditDistance)
                return 1;
            else
                return 0;
        }
    }
}
