using System.Collections.Generic;
using System.Linq;
using Replica.Core.Exceptions;
using Replica.Core.Modules;

namespace Replica.Core.Flows
{
    public class FlowsModule : IModule
    {
        private IList<IFlow> _flows = new List<IFlow>();

        public void RegisterFlow<T>() where T : IFlow, new()
            => _flows.Add(new T());

        public IFlow ResolveFlow(string name)
            => _flows.FirstOrDefault(x => x.Name == name)?.Clone()
                ?? throw new NotRegisteredException("Flow not registered");

        void IModule.Init(BotCore core)
        {
            core.EnableAssemblyLocalization();
            core.Router.AddHandler<FlowsHandler>();
        }
    }
}