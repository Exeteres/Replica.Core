namespace Replica.Core.Flows
{
    public struct FlowButton
    {
        public FlowButton(string label, string method)
        {
            Label = label;
            Method = method;
        }

        internal string Label { get; set; }
        internal string Method { get; set; }
    }
}