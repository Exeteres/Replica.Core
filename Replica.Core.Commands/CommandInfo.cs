using Replica.Core.Commands.Utils;

namespace Replica.Core.Commands
{
    public class CommandInfo
    {
        public string Command { get; set; }
        public string[] Aliases { get; set; }
        public ActionInfo[] Actions { get; set; }
        public string Usage { get; set; }
        public IRestriction Restriction { get; set; }
    }
}
