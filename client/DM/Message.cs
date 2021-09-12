using System;
using System.Collections.Generic;
using System.Text;

namespace DM
{
    public class Message
    {
        public bool? Content { get; set; }
        public Message(bool? _content)
        {
            Content = _content;
        }
    }
}
