using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Replica.Core.Contexts;
using Replica.Core.Entity;
using Replica.Core.Exceptions;
using Replica.Core.Extensions;
using Replica.Core.Messages;
using Replica.Core.Utils;

namespace Replica.Core.Flows
{
    public class FlowBase : IFlow
    {
        protected Context Context { get; private set; }

        private FlowInfo _info;

        public string Name => _info.Name;
        protected object[] Arguments { get; private set; }

        private FlowInternalButton[] _state;
        FlowInternalButton[] IFlow.State => _state;

        protected string Message { get; private set; }

        private Stack<MethodInfo> _history { get; set; } = new Stack<MethodInfo>();

        public void Leave(string message)
        {
            var msg = OutMessage.FromText(message);
            msg.Keyboard = new Keyboard { Remove = true };
            Context.SendMessage(msg);
        }

        void IFlow.Rollback()
        {
            _history.Pop();
            SendFlowNode(_history.Pop());
        }

        protected bool BackButton { get; set; }
        protected bool CancelButton { get; set; }

        public FlowBase()
        {
            _info = Helpers.ExtractMetaInfo<FlowInfo>(this.GetType());
            if (string.IsNullOrEmpty(_info.Name))
                throw new MissedInfoException("");
            _info.Methods = this.GetType().GetMethods();
            _info.Default = _info.Methods.FirstOrDefault(x => x.Name.ToLower() == "default")
                ?? throw new MissingMethodException("Default method not found");
        }

        void IFlow.Enter(Context context, params object[] args)
        {
            if (FlowsHandler.Flows.GetValue(context.Message.Chat.Id) != null)
                return;
            FlowsHandler.Flows[context.Message.Chat.Id] = this;

            Context = context;
            Arguments = args;
            SendFlowNode(_info.Default);
        }

        public void Process(string message, MethodInfo method)
        {
            Message = message;
            SendFlowNode(method);
        }

        private void SendFlowNode(MethodInfo method)
        {
            _history.Push(method);

            var resp = (FlowResponse)method.Invoke(this, null);
            var message = resp.Builder.Build(Context);

            var markup = resp.Markup.Buttons.Select(x => x.Select(y => y.Label).ToArray()).ToList();

            if (BackButton || CancelButton)
            {
                var localizer = Context.GetLocalizer();
                var row = new List<string>();
                if (method.Name.ToLower() != "default" && BackButton)
                    row.Add(localizer["Back"]);
                if (CancelButton)
                    row.Add(localizer["Cancel"]);
                markup.Add(row.ToArray());
            }

            message.Keyboard = new Keyboard
            {
                Markup = markup.ToArray()
            };

            _state = resp.Markup.Buttons.Select(x => x.Select(y => new FlowInternalButton
            {
                Label = y.Label,
                Method = _info.Methods.FirstOrDefault(z => z.Name.ToLower() == y.Method)
            })).Aggregate((a, b) => a.Concat(b)).ToArray();

            Context.SendMessage(message);
        }

        public IFlow Clone()
        {
            return (IFlow)MemberwiseClone();
        }
    }
}