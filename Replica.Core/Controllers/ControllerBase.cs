using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Replica.Core.Caching;
using Replica.Core.Contexts;
using Replica.Core.Entity;
using Replica.Core.Entity.Attachments;
using Replica.Core.Messages;
using Replica.Core.Utils;
using Serilog;

namespace Replica.Core.Controllers
{
    public abstract class ControllerBase<TOptions> : IController, IInternalController where TOptions : new()
    {
        private readonly ControllerInfo _info;

        private PersistentCache _cache;

        private string GenerateKey(Attachment attachment)
        {
            return $"{_info.Name}_{attachment.FileId}";
        }

        protected string GetContentId(Attachment attachment)
        {
            if (_cache == null || attachment.Controller == _info.Name)
                return attachment.FileId;
            return _cache.Get(GenerateKey(attachment));
        }

        protected void SetContentId(Attachment attachment, string id)
        {
            if (_cache == null) return;
            _cache.Set(GenerateKey(attachment), id);
        }

        private BotCore _core;

        protected ControllerBase()
        {
            _info = Helpers.ExtractMetaInfo<ControllerInfo>(GetType());
        }

        protected IController ResolveController(string name)
        {
            return _core.ResolveController(name);
        }

        protected TOptions Options { get; private set; }

        public string Name => _info.Name;

        public abstract Task<IEnumerable<long>> SendMessage(long chatId, OutMessage message);
        public abstract void Start();
        public abstract void Stop();
        public abstract void Init();

        void IInternalController.SetCore(BotCore core)
        {
            _core = core;
            _cache = core.CreateCache<string>(Name);
        }

        void IInternalController.SetOptions(JToken options)
        {
            Options = options.ToObject<TOptions>();
        }

        #region Sessions

        private ConcurrentDictionary<(Type, object), object> _sessions = new ConcurrentDictionary<(Type, object), object>();

        public T ResolveSession<T>(object key) where T : new()
        {
            return (T)_sessions.GetOrAdd((typeof(T), key), new T());
        }

        #endregion

        protected string RegisterHandler(ButtonHandler handler)
        {
            return _core.RegisterHandler(handler);
        }

        public abstract Task DeleteMessage(long chatId, int id);

        protected void TakeOver(InMessage origin)
        {
            _core.TakeOver(this, origin);
        }

        protected void TakeOverButtons(InlineQuery query, string id)
        {
            _core.TakeOverButtons(query, id);
        }

        public abstract Task EditMessage(long chatId, long id, OutMessage message);
        public abstract Task<string> ResolveSource(Attachment attachment);


        protected abstract Task<UserInfo> FetchUserInfo(long userId);
        protected abstract Task<ChatInfo> FetchChatInfo(long chatId);

        protected abstract void SetupWebhook(SimpleHttpServer server, string endpoint, string path, string secret);
        protected abstract void DisposeWebhook();

        public void InitWebhook()
        {
            try
            {
                var endpoint = _core.Endpoint.TrimEnd('/') + "/" + Name;
                var path = "/" + string.Join("/", endpoint.Split('/').Skip(3));
                SetupWebhook(_core.WebhookServer, endpoint, path, Helpers.GenerateString(42));
            }
            catch (NotImplementedException) { }
        }

        private readonly ConcurrentDictionary<long, Task<UserInfo>> _users = new ConcurrentDictionary<long, Task<UserInfo>>();
        private readonly ConcurrentDictionary<long, Task<ChatInfo>> _chats = new ConcurrentDictionary<long, Task<ChatInfo>>();

        public Task<UserInfo> GetUserInfo(long userId)
        {
            Log.Debug("Requested GetUserInfo {UserId}", userId);
            return _users.GetOrAdd(userId, x => FetchUserInfo(userId));
        }

        public Task<ChatInfo> GetChatInfo(long chatId)
        {
            Log.Debug("Requested GetChatInfo {ChatId}", chatId);
            return _chats.GetOrAdd(chatId, x => FetchChatInfo(chatId));
        }

        public void Dispose()
        {
            DisposeWebhook();
        }
    }
}
