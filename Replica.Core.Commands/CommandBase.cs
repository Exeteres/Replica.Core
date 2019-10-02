using System;
using System.Linq;
using System.Threading.Tasks;

using Replica.Core.Commands.Utils;
using Replica.Core.Entity;
using Replica.Core.Extensions;
using Replica.Core.Handlers;
using Replica.Core.Messages;
using Replica.Core.Utils;

namespace Replica.Core.Commands
{
    public abstract class CommandBase : HandlerBase
    {
        internal CommandInfo Info { get; private set; }

        public CommandBase()
        {
            Info = Helpers.ExtractMetaInfo<CommandInfo>(this.GetType());
            Info.Restriction = this.GetType().GetAttribute<IRestriction>();
            Info.Actions = ActionInfo.FromType(this.GetType(), Info.Command);
            Info.Usage = $"/{Info.Command} {{{string.Join("|", Info.Actions.Where(x => x.Name != "Default").Select(x => x.Name.ToLower()).Distinct())}}}";
        }

        private async void HandleResult(object result)
        {
            switch (result)
            {
                case null:
                    break;
                case MessageBuilder builder:
                    await Context.SendMessage(builder.Build(Context));
                    break;
                case OutMessage message:
                    await Context.SendMessage(message);
                    break;
                case Task<MessageBuilder> task:
                    var build = await task;
                    if (build == null) break;
                    await Context.SendMessage(build.Build(Context));
                    break;
                case Task<OutMessage> task:
                    var msg = await task;
                    if (msg == null) return;
                    await Context.SendMessage(msg);
                    break;
            }
        }

        private static bool TryParseCommand(string text, out (string, string[]) info)
        {
            info.Item1 = null;
            info.Item2 = null;
            if (text == null)
                return false;
            if (!text.StartsWith("/"))
                return false;
            var parts = text.Split(' ');
            info.Item1 = parts[0].TrimStart('/');
            info.Item2 = parts.Skip(1).ToArray();
            return true;
        }

        protected override void TakeOver()
        {
            var localizer = Context.GetLocalizer();

            if (!TryParseCommand(Message.Text, out var info) || (info.Item1.ToLower() != Info.Command && !Info.Aliases.Contains(info.Item1)))
            {
                base.TakeOver();
                return;
            }

            if (Info.Restriction != null && !Info.Restriction.Check(Context))
            {
                Context.SendMessage(OutMessage.FromCode(localizer["AccessDenied"]));
                return;
            }

            void Invoke(ActionInfo act, string[] args)
            {
                if (act.Restriction != null && !act.Restriction.Check(Context))
                {
                    Context.SendMessage(OutMessage.FromCode(localizer["AccessDenied"]));
                    return;
                }

                var message = act.TryParseArguments(Context, args, out var parameters);
                if (message != null)
                {
                    Context.SendMessage(OutMessage.FromCode(message + "\nUsage: " + act.Usage));
                    return;
                }

                HandleResult(act.Invoke(this, parameters));
            }

            var (command, arguments) = info;

            var arg = arguments.FirstOrDefault();
            if (string.IsNullOrEmpty(arg) && !Info.Actions.Any())
                return;

            var actions = Info.Actions.ToList().FindAll(x => x.Name.ToLower() == arg?.ToLower());
            if (!actions.Any())
            {
                var df = Info.Actions.FirstOrDefault(x => x.Name.ToLower() == "default");
                if (df == null)
                {
                    Context.SendMessage(OutMessage.FromCode($"Action{(string.IsNullOrEmpty(arg) ? "" : " " + arg)} not found\nUsage: " + Info.Usage));
                    return;
                }
                Invoke(df, arguments);
                return;
            }

            var action = actions.Aggregate((x, y) =>
                Math.Abs(x.Required - arguments.Length + 1) <
                Math.Abs(y.Required - arguments.Length + 1) ? x : y);

            Invoke(action, arguments.Skip(1).ToArray());
        }
    }
}
