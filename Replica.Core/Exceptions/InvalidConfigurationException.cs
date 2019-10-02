using System;

namespace Replica.Core.Exceptions
{
    [Serializable]
    public class InvalidConfigurationException : Exception
    {
        public InvalidConfigurationException(string message) : base(message) { }
    }
}
