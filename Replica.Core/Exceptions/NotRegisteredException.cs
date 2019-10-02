using System;

namespace Replica.Core.Exceptions
{
    [Serializable]
    public class NotRegisteredException : Exception
    {
        public NotRegisteredException(string message) : base(message) { }
    }
}
