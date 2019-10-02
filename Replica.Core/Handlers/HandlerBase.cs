using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Replica.Core.Contexts;
using Replica.Core.Controllers;
using Replica.Core.Entity;
using Replica.Core.Utils;

namespace Replica.Core.Handlers
{
    public abstract class HandlerBase : IHandler
    {
        private IHandler _next;

        public Context Context { get; private set; }

        // Shortcuts
        protected InMessage Message => Context.Message;
        protected IController Controller => Context.Controller;
        protected BotCore Core => Context.Core;

        IHandler IHandler.SetNext(IHandler next)
        {
            _next = next;
            return next;
        }

        protected virtual void TakeOver() => Next();
        private void Next() => _next?.Process(Context);

        public void Process(Context context)
        {
            Context = context;
            TakeOver();
        }

        public IHandler Clone() => (IHandler)MemberwiseClone();
    }
}
