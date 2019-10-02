using System;

namespace Replica.Core.Exceptions
{
    [Serializable]
    public class MissedInfoException : Exception
    {
        public MissedInfoException(string message) : base(message) { }
    }
}
