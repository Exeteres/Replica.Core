using System.Collections.Generic;
using System.Linq;
using Replica.Core.Exceptions;
using Replica.Core.Modules;

namespace Replica.Core.Views
{
    public class ViewsModule : IModule
    {
        private IList<IView> _views = new List<IView>();

        public void RegisterView<T>() where T : IView, new()
            => _views.Add(new T());

        public IView ResolveView(string name)
            => _views.FirstOrDefault(x => x.Name == name)?.Clone()
                ?? throw new NotRegisteredException("View not registered");

        void IModule.Init(BotCore core) { }
    }
}