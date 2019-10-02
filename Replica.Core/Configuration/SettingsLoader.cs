using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Replica.Core.Exceptions;

namespace Replica.Core.Configuration
{
    public class SettingsLoader<T> : ISettingsLoader where T : class
    {
        internal readonly Settings<T> _settings = new Settings<T>();

        private SettingsLoader() { }
        ISettings ISettingsLoader.Settings => _settings;

        public T Options => _settings.Options;

        public static SettingsLoader<T> FromJObject(JObject obj)
        {
            var loader = new SettingsLoader<T>();
            loader._settings.Controllers = obj.GetValue("controllers");
            loader._settings.Handlers = obj.GetValue("handlers");
            loader._settings.Core = obj.GetValue("core")?.ToObject<CoreOptions>();
            loader._settings.Options = obj.GetValue("options")?.ToObject<T>();
            return loader;
        }

        public static SettingsLoader<T> FromFile(string path)
        {
            if (!File.Exists(path))
                throw new InvalidConfigurationException("Missing settings.json file");

            var json = File.ReadAllText(path);
            return FromJObject(JObject.Parse(json));

        }
    }
}
