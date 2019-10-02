using System;

namespace Replica.Core.Exceptions
{
    [Serializable]
    public class MissedActionException : Exception
    {
        public MissedActionException(string message) : base(message) { }
    }
}
