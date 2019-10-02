using System.Collections.Generic;
using System.Linq;
using Replica.Core.Handlers;

namespace Replica.Core.Routing
{
    public sealed class Router : IRouter
    {
        private BotCore _core;

        internal Router(BotCore core)
        {
            _core = core;
        }

        private readonly IList<object> _handlers = new List<object>();

        public IEnumerable<IHandler> GetHandlers()
            => _handlers.Where(x => x is IHandler).Cast<IHandler>();

        public void AddHandler<T>() where T : IHandler, new()
        {
            _handlers.Add(new T());
        }

        public void AddRouter(IRouter router)
        {
            _handlers.Add(router);
        }

        public IEnumerable<IHandler> BuildHandlersChain()
        {
            var handlers = new List<IHandler>();
            foreach (var handler in _handlers)
                switch (handler)
                {
                    case IRouter routes:
                        handlers.AddRange(routes.BuildHandlersChain());
                        break;
                    case IHandler entry:
                        handlers.Add(entry.Clone());
                        break;
                }

            return handlers;
        }
    }
}
