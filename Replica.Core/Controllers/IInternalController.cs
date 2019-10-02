using System;
using Newtonsoft.Json.Linq;

namespace Replica.Core.Controllers
{
    public interface IInternalController : IController, IDisposable
    {
        void Init();
        void InitWebhook();
        void SetCore(BotCore core);
        void SetOptions(JToken options);
    }
}