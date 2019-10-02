using System;

namespace Replica.Core.Flows
{
    public class FlowAttribute : Attribute
    {
        public FlowInfo Info { get; }

        public FlowAttribute(string name)
        {
            Info = new FlowInfo { Name = name };
        }
    }
}