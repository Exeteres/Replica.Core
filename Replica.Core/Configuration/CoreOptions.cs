using Newtonsoft.Json.Linq;

namespace Replica.Core.Configuration
{
    public class CoreOptions
    {
        public string Language { get; set; } = "en";
        public string Endpoint { get; set; }
        public int Port { get; set; } = 4000;
        public string Cache { get; set; }
        public JToken Log { get; set; }
    }
}
