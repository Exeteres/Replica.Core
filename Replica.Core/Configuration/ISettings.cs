using Newtonsoft.Json.Linq;

namespace Replica.Core.Configuration
{
    public interface ISettings
    {
        JToken Controllers { get; }
        JToken Handlers { get; }
        object Options { get; }
        CoreOptions Core { get; }
    }
}
