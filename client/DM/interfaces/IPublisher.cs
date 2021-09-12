using System;
using System.Collections.Generic;
using System.Text;

namespace DM
{
    //This is for the ui. There are triples where a
    //variable appears more than once. We only show
    //a single instance and the other ones are updated
    //accordingly. We use the sub-pub pattern for this.
    public interface IPublisher
    {
        event EventHandler<Message> Handler;
        void Publish();
    }
}
