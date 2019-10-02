using System.Collections.Generic;

namespace Replica.Core.Handlers
{
    public class HandlerInfo
    {
        public IEnumerable<IHandler> PreHandlers { get; set; }
        public IEnumerable<IHandler> PostHandlers { get; set; }
    }
}
