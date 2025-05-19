using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BannanagramsLibrary
{
    public class ClientToServerMessage
    {
        public ClientToServerMessageType Type { get; set; }
        public object? Payload { get; set; }
    }
}