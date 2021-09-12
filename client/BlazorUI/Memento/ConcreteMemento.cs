using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorUI
{
    using Simon;
    public class ConcreteMemento : IMemento
    {
        public Simon Simon { get; set; }

        public Interaction Interaction { get; set; }

        public ConcreteMemento(Simon simon, Interaction interaction)
        {
            this.Simon = simon;
            this.Interaction = interaction;
        }

        // The Originator uses this method when restoring its state.
        public (Simon, Interaction) GetState()
        {
            return (this.Simon, this.Interaction);
        }        
    }

    // The Caretaker doesn't depend on the Concrete Memento class. Therefore, it
    // doesn't have access to the originator's state, stored inside the memento.
    // It works with all mementos via the base Memento interface.    
}
