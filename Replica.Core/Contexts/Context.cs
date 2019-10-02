using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

using Replica.Core.Controllers;
using Replica.Core.Entity;
using Replica.Core.Localization;

namespace Replica.Core.Contexts
{
    public sealed class Context
    {
        public BotCore Core { get; }
        public InMessage Message { get; }

        public IController Controller { get; private set; }

        public Context(BotCore core, IController controller, InMessage message)
        {
            Core = core;
            Controller = controller;
            Message = message;
        }

        public Task<IEnumerable<long>> SendMessage(OutMessage message)
        {
            return Controller.SendMessage(Message.Chat.Id, message);
        }

        public Task<IEnumerable<long>> SendMessage(string message)
        {
            return Controller.SendMessage(Message.Chat.Id, OutMessage.FromText(message));
        }

        public Localizer GetLocalizer()
            => Core.ResolveLocalizer(Assembly.GetCallingAssembly(), Message.Sender.Language);

        public T ResolveSession<T>() where T : new()
        {
            return Controller.ResolveSession<T>(Message.Chat.Id + "_" + Message.Id);
        }
    }
}
