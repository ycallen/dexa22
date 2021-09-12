using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorUI
{
    public class Caretaker
    {
        private List<IMemento> _mementos = new List<IMemento>();

        private Originator _originator = null;

        public int InteractionCount { get { return this._mementos.Count; } }

        public Caretaker(Originator originator)
        {
            this._originator = originator;
        }

        public void Backup()
        {
            this._mementos.Add(this._originator.Save());
        }

        
        public void Undo()
        {
            if (this._mementos.Count == 0)
            {
                return;
            }

            var memento = this._mementos.Last();
            this._mementos.Remove(memento);

            try
            {
                this._originator.Restore(memento);
            }
            catch (Exception)
            {
                this.Undo();
            }
        }
    }
}
