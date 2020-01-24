using System.Globalization;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;

using Replica.Core.Caching;
using Replica.Core.Configuration;
using Replica.Core.Contexts;
using Replica.Core.Controllers;
using Replica.Core.Entity;
using Replica.Core.Exceptions;
using Replica.Core.Extensions;
using Replica.Core.Localization;
using Replica.Core.Routing;
using Replica.Core.Utils;
using Replica.Core.Modules;
using Serilog.Core;
using Serilog;
using System.Reflection;

namespace Replica.Core
{
    public class BotCore : IDisposable
    {
        private readonly IList<IController> _controllers = new List<IController>();

        public IRouter Router { get; }
        private readonly ISettings _settings;

        private readonly Dictionary<string, ButtonHandler> _entries = new Dictionary<string, ButtonHandler>();
        private ICacheProvider _cacheProvider;
        private readonly IList<LanguageManager> _locales = new List<LanguageManager>();

        internal SimpleHttpServer WebhookServer { get; private set; }
        internal string Endpoint => _settings.Core.Endpoint;

        internal string DefaultLanguage => _settings.Core.Language;

        public BotCore(ISettingsLoader loader)
        {
            Router = new Router(this);
            _settings = loader?.Settings
                ?? throw new ArgumentNullException(nameof(loader));
            if (_settings.Core.Cache != null)
                Directory.CreateDirectory(_settings.Core.Cache);

            if (string.IsNullOrEmpty(_settings.Core.Endpoint)) return;
            WebhookServer = new SimpleHttpServer(_settings.Core.Port);
        }

        public void EnableAssemblyLocalization()
        {
            var locale = new LanguageManager(Assembly.GetCallingAssembly(), _settings.Core.Language);
            _locales.Add(locale);
        }

        public Localizer ResolveLocalizer(Assembly asm, string name)
        {
            var manager = _locales.First(x => x.Assembly == asm);
            return manager.CreateLocalizer(string.IsNullOrEmpty(name) ? _settings.Core.Language : name);
        }

        public IController ResolveController(string name)
        {
            return _controllers.FirstOrDefault(c => c.Name == name)
                ?? throw new NotRegisteredException("Controller not registered");
        }

        public void RegisterController<T>() where T : IInternalController, new()
        {
            var controller = new T();
            if (controller.Name == null)
                throw new MissedInfoException("Missed controller attribute");
            if (_controllers.Any(c => c.Name == controller.Name))
                throw new AlreadyRegisteredException("Controller with same name already registered");
            var options = _settings.Controllers?[controller.Name];
            if (options == null) return;
            controller.SetCore(this);
            controller.SetOptions(options);
            controller.Init();
            if (WebhookServer != null)
                controller.InitWebhook();
            _controllers.Add(controller);
        }

        public void TakeOver(IController controller, InMessage message)
        {
            Log.Information("Received message {Id} from {User} in chat {Chat} via {Controller} controller",
                message.Id,
                message.Sender.Id,
                message.Chat.Id,
                controller.Name);
            var context = new Context(this, controller, message);

            var chain = Helpers.LinkHandlers(Router.BuildHandlersChain());
            chain.Process(context);
        }

        public string RegisterHandler(ButtonHandler handler)
        {
            return _entries.Save(handler);
        }

        public void TakeOverButtons(InlineQuery query, string id)
        {
            var entry = _entries.GetValue(id);
            if (entry == null)
                return;
            entry.Invoke(query);
            //_entries.Remove(id);
        }

        public void Start()
        {
            if (WebhookServer != null)
            {
                WebhookServer.Listen();

                // Fix discord longpoll
                var dcController = _controllers.FirstOrDefault(x => x.Name == "dc");
                dcController?.Start();
                return;
            }
            foreach (var controller in _controllers)
                controller.Start();
        }

        public void EnableCaching<T>() where T : ICacheProvider, new()
        {
            _cacheProvider = new T();
        }

        internal PersistentCache CreateCache<T>(string suffix)
        {
            if (_settings.Core.Cache == null) return null;
            return new PersistentCache(_cacheProvider, Path.Combine(_settings.Core.Cache, suffix));
        }

        public void Stop()
        {
            if (WebhookServer != null)
            {
                WebhookServer.Stop();

                // Fix discord longpoll
                var dcController = _controllers.FirstOrDefault(x => x.Name == "dc");
                dcController?.Stop();
                return;
            }
            foreach (var controller in _controllers)
                controller.Stop();
        }

        #region Modules

        private readonly IList<(Type, IModule)> _modules = new List<(Type, IModule)>();

        public T RegisterModule<T>() where T : IModule, new()
        {
            var module = new T();
            module.Init(this);
            _modules.Add((typeof(T), module));
            return module;
        }

        public T ResolveModule<T>() where T : IModule, new()
        {
            return (T)_modules.FirstOrDefault(x => x.Item1 == typeof(T)).Item2
                ?? throw new NotRegisteredException("Module not registered");
        }

        public void Dispose()
        {
            Stop();
            foreach (var controller in _controllers)
                ((IInternalController)controller).Dispose();
        }

        #endregion
    }
}
