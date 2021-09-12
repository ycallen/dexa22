using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorUI
{
    using Simon;
    //this is for the ui. We use the memento pattern 
    //for backtracking
    public interface IMemento
    {
        (Simon, Interaction) GetState();     
    }
}
