using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library
{
    public class ServerToClientMessage
    {
        public ServerToClientMessageType Type { get; set; }
        public object? Payload { get; set; }
    }
}