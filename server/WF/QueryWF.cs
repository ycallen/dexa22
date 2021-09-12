using BL;
using DM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace WF
{
    public class QueryWF
    {
        public List<DM.Triple> ExtractTriplesFromQuery(TripleQuery query)
        {
            var dt = new QueryBL().ExtractTriplesFromQuery(query);
            return dt;
        }

        public Dictionary<string,List<string>> GetResultsFromQuery(TripleQuery query)
        {
            var dt = new QueryBL().GetResultsFromQuery(query);
            return dt;
        }

    }
}
