using System.Reflection;
using System;
using System.Linq;
using Replica.Core.Contexts;
using Replica.Core.Messages;
using Replica.Core.Utils;
using Replica.Core.Exceptions;

namespace Replica.Core.Views
{
    public class ViewBase : IView
    {
        private Context _context;
        private MethodInfo _state;
        private ViewInfo _info;
        private long? _message;

        public ViewBase()
        {
            _info = Helpers.ExtractMetaInfo<ViewInfo>(this.GetType());
            if (string.IsNullOrEmpty(_info.Name))
                throw new MissedInfoException("");
            _info.Methods = this.GetType().GetMethods();
            _info.Default = _info.Methods.FirstOrDefault(x => x.Name.ToLower() == "default")
                ?? throw new MissingMethodException("Default method not found");
        }

        protected Context Context => _context;

        protected virtual void Init(object[] args) { }

        public string Name => _info.Name;

        protected void ChangeState(string state)
        {
            _state = _info.Methods.FirstOrDefault(x => x.Name == state.ToLower());
            UpdateView();
        }

        void IView.Enter(Context context, params object[] args)
        {
            _context = context;
            _state = _info.Default;
            Init(args);
            UpdateView();
        }

        protected async void Destroy()
        {
            if (_message.HasValue)
                await _context.Controller.DeleteMessage(_context.Message.Chat.Id, (int)_message.Value);
        }

        protected async void UpdateView()
        {

            var builder = (MessageBuilder)_state.Invoke(this, null);
            var message = builder.Build(_context);
            if (!_message.HasValue)
            {
                var ids = await _context.Controller.SendMessage(_context.Message.Chat.Id, message);
                _message = ids.Last();
            }
            else
            {
                await _context.Controller.EditMessage(_context.Message.Chat.Id, _message.Value, message);
            }
        }

        public IView Clone()
        {
            return (IView)MemberwiseClone();
        }
    }
}
