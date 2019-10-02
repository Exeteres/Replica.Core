using Replica.Core.Contexts;

namespace Replica.Core.Flows
{
    public static class ContextExtensions
    {
        public static void EnterFlow(this Context context, string name, params object[] args)
        {
            context.Core.ResolveModule<FlowsModule>()
                .ResolveFlow(name)
                .Enter(context, args);
        }
    }
}