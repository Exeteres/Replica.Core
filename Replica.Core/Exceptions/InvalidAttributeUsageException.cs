using System;
namespace Replica.Core.Exceptions
{
    [Serializable]
    public class InvalidAttributeUsageException : Exception
    {
        public InvalidAttributeUsageException(string message) : base(message) { }
    }
}