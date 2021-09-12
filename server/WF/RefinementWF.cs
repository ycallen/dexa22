using BL;
using DM;
using Nest;
using System;
using System.Collections.Generic;
using System.Data;

namespace WF
{
    public class RefinementWF
    {
       
        public SearchResponse GetCandidates(TripleQuery query)
        {
            var data = new RefinementBL().GetCandidates(query);
            return data;
        }
    }
}
