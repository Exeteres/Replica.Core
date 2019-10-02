using Newtonsoft.Json.Linq;

namespace Replica.Controllers.VK
{
    public class CallbackUpdate
    {
        public string Type { get; set; }
        public JObject Object { get; set; }
        public long GroupId { get; set; }
        public string Secret { get; set; }
    }
}