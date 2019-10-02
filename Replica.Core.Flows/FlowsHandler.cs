using System.Linq;
using System.Collections.Generic;
using Replica.Core.Handlers;
using Replica.Core.Extensions;

namespace Replica.Core.Flows
{
    public class FlowsHandler : HandlerBase
    {
        internal static Dictionary<long, IFlow> Flows = new Dictionary<long, IFlow>();

        protected override void TakeOver()
        {
            if (string.IsNullOrEmpty(Context.Message.Text))
            {
                base.TakeOver();
                return;
            }

            var flow = Flows.GetValue(Context.Message.Chat.Id);
            if (flow == null)
            {
                base.TakeOver();
                return;
            }

            var localizer = Context.GetLocalizer();

            if (Context.Message.Text == localizer["Back"])
            {
                flow.Rollback();
                return;
            }

            if (Context.Message.Text == localizer["Cancel"])
            {
                flow.Leave(localizer["Cancelled"]);
                Flows.Remove(Context.Message.Chat.Id);
                return;
            }

            var btn = flow.State.FirstOrDefault(x => x.Label == Context.Message.Text);
            if (btn.Method == null)
            {
                base.TakeOver();
                return;
            }

            flow.Process(Context.Message.Text, btn.Method);
        }
    }
}