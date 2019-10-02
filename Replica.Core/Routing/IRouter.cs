using System.Collections.Generic;
using Replica.Core.Handlers;

namespace Replica.Core.Routing
{
    public interface IRouter
    {
        void AddHandler<T>() where T : IHandler, new();
        void AddRouter(IRouter router);
        IEnumerable<IHandler> GetHandlers();
        IEnumerable<IHandler> BuildHandlersChain();
    }
}
