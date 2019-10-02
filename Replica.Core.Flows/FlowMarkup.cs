using System.Linq;
namespace Replica.Core.Flows
{
    public struct FlowMarkup
    {
        public FlowMarkup(FlowButton[] buttons)
        {
            Buttons = new FlowButton[][] { buttons };
        }

        public FlowMarkup(string method, string[] buttons)
        {
            Buttons = new FlowButton[][] { buttons.Select(x => new FlowButton(x, method)).ToArray() };
        }

        public FlowMarkup(string method, string[][] buttons)
        {
            Buttons = buttons.Select(x => x.Select(y => new FlowButton(y, method)).ToArray()).ToArray();
        }

        public FlowMarkup(FlowButton[][] buttons)
        {
            Buttons = buttons;
        }

        internal FlowButton[][] Buttons { get; set; }
    }
}