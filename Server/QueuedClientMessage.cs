using Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class QueuedClientMessage
    {
        public ClientToServerMessage Message { get; set; } = default!;
        public string ClientId { get; set; } = string.Empty;
    }
}
