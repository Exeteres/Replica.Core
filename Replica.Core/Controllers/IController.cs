using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Replica.Core.Contexts;
using Replica.Core.Entity;
using Replica.Core.Entity.Attachments;
using Replica.Core.Messages;

namespace Replica.Core.Controllers
{
    public interface IController
    {
        string Name { get; }

        void Start();
        void Stop();

        Task<IEnumerable<long>> SendMessage(long chatId, OutMessage message);
        Task DeleteMessage(long chatId, int id);
        Task EditMessage(long chatId, long id, OutMessage message);

        Task<string> ResolveSource(Attachment attachment);
        T ResolveSession<T>(object key) where T : new();

        Task<UserInfo> GetUserInfo(long userId);
        Task<ChatInfo> GetChatInfo(long chatId);
    }
}
