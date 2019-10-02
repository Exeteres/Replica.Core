using System;

namespace Replica.Core.Entity
{
    [Flags]
    public enum MessageFlags
    {
        Code = 1,
        AllowHTMl = 2
    }
}