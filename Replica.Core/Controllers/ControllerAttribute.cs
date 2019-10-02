using System;

namespace Replica.Core.Controllers
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ControllerAttribute : Attribute
    {
        public ControllerAttribute(string name)
        {
            Info = new ControllerInfo { Name = name };
        }

        public ControllerInfo Info { get; }
    }
}
