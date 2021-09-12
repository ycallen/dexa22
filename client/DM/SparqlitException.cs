using System;
using System.Collections.Generic;
using System.Text;

namespace DM
{
    public class SparqlitException : Exception
    {
        public Guid Guid { get; set; }

        public string ErrorDetail { get; set; }

        public string Error { get; set; }

        public SparqlitException()
        {
        }               
    }
}
