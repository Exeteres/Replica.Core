using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Replica.Core.Entity
{
    public struct ChatInfo
    {
        public ChatInfo(long id, string title, long owner)
        {
            Id = id;
            Title = title;
            Owner = owner;
        }

        public long Id { get; private set; }
        public string Title { get; private set; }
        public long Owner { get; private set; }
    }
}