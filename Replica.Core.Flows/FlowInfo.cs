using System.Reflection;

namespace Replica.Core.Flows
{
    public class FlowInfo
    {
        public string Name { get; set; }
        public MethodInfo[] Methods { get; set; }
        public MethodInfo Default { get; set; }
    }
}