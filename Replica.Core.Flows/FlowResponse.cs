using Replica.Core.Messages;

namespace Replica.Core.Flows
{
    public struct FlowResponse
    {
        public FlowResponse(MessageBuilder builder, FlowMarkup markup)
        {
            Builder = builder;
            Markup = markup;
        }

        internal MessageBuilder Builder { get; set; }
        internal FlowMarkup Markup { get; set; }
    }
}