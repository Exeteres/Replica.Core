using Replica.Core.Handlers;

namespace Replica.Core.Views
{
    public static class IHandlerExtensions
    {
        public static void EnterView(this IHandler handler, string name, params object[] args)
        {
            handler.Context.Core
                .ResolveModule<ViewsModule>()
                .ResolveView(name)
                .Enter(handler.Context, args);
        }
    }
}