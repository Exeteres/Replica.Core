using System.Reflection;
using Replica.Core.Utils;

namespace Replica.Core.Localization
{
    public class LanguageManager
    {
        private JsonResourceManager _jrm;

        public Assembly Assembly { get; private set; }

        public LanguageManager(Assembly asm, string df)
        {
            _jrm = new JsonResourceManager(asm, df);
            Assembly = asm;
        }

        public Localizer CreateLocalizer(string key)
            => new Localizer(_jrm.Resolve(key), key);
    }
}