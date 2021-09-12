using DM;
using Force.DeepCloner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simon
{
    public interface ISelector
    {
         public int Count();        

        public bool IsEmpty();

        public Candidate GetNext();        
        
        public void FilterByFeedback(Candidate candidate);
        
    }
}
