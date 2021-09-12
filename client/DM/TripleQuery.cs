using Caching;
using Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DM
{
    public class TripleQuery 
    {
        public TripleQuery()
        {
            Header = Utils.GetHeader();
            Size = 10;
        }

        public String Header { get; set; }
        public String Body { get; set; }

        public int? JenaLimit { get; set; }

        public int? ESLimitEntities { get; set; }

        public int? ESLimitLiterals { get; set; }        

        public int? QueryTimeout { get; set; }

        public bool IsEditDistance { get; set; }
        

        public bool IsUseSynonym { get; set; }

        public bool IsEsEditDistance { get; set; }

        public bool IsUnitTest { get; set; }

        public bool IsPartial { get; set; }

        public List<Triple> Triples { get; set; }

        public int Size { get; set; }
        public string Url { get; set; }

        public Guid Guid { get; set; }

    }
}
