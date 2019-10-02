using System;

namespace Replica.Core.Exceptions
{
    [Serializable]
    public class AlreadyRegisteredException : Exception
    {
        public AlreadyRegisteredException(string message) : base(message) { }
    }
}
