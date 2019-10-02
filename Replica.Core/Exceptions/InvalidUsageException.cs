using System;

namespace Replica.Core.Exceptions
{
    [Serializable]
    public class InvalidUsageException : Exception
    {
        public InvalidUsageException(string message) : base(message) { }
    }
}
