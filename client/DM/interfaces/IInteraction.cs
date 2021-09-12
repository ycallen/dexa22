using System;
using System.Collections.Generic;
using System.Text;

namespace DM
{
    public interface IInteraction
    {
        public int? JenaLimit { get; set; }
        public int? ESLimitEntities { get; set; }

        public int? ESLimitLiterals { get; set; }

        public int? QueryTimeout { get; set; }


        public bool IsEditDistance { get; set; }

        public bool IsUnitTest { get; set; }

        public bool IsUseSynonym { get; set; }

        public bool IsEsEditDistance { get; set; }

        public bool IsPartial { get; set; }
    }
}
