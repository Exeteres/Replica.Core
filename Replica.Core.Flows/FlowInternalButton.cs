using System.Reflection;
namespace Replica.Core.Flows
{
    public struct FlowInternalButton
    {
        public string Label { get; set; }
        public MethodInfo Method { get; set; }
    }
}