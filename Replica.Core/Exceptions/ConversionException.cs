using System;

namespace Replica.Core.Exceptions
{
    [Serializable]
    public class ConversionException : Exception
    {
        public ConversionException(string message) : base(message) { }
    }
}
