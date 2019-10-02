using System.Linq;
using System;
using System.Collections.Generic;
using Replica.Core.Exceptions;
using Replica.Core.Modules;

namespace Replica.Core.Commands
{
    public class CommandsModule : IModule
    {
        private BotCore _core;

        private IList<IConverter> _converters = new List<IConverter>();

        public void RegisterConverter<T>() where T : IConverter, new()
            => _converters.Add(new T());

        public IConverter ResolveConverter(Type t)
            => _converters.FirstOrDefault(x => x.Type == t)
                ?? throw new NotRegisteredException("Converter not registered");

        public IEnumerable<CommandInfo> GetCommands()
            => _core.Router.GetHandlers().Where(x => x is CommandBase)
                .Cast<CommandBase>().Select(x => x.Info);

        void IModule.Init(BotCore core)
        {
            _core = core;
            core.EnableAssemblyLocalization();
        }
    }
}