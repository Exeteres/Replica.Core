using System;

namespace Replica.Core.Commands
{
    public class CommandAttribute : Attribute
    {
        public CommandAttribute(string command, params string[] aliases)
        {
            Info = new CommandInfo { Command = command, Aliases = aliases };
        }

        internal CommandInfo Info { get; }
    }
}
