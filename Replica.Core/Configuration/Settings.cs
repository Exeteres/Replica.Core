using Newtonsoft.Json.Linq;

namespace Replica.Core.Configuration
{
    public class Settings<T> : ISettings
    {
        public T Options { get; set; }
        public JToken Controllers { get; set; }
        public JToken Handlers { get; set; }
        public CoreOptions Core { get; set; }
        object ISettings.Options => Options;
    }
}
