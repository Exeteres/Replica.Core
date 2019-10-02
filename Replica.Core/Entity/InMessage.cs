using System;
using Destructurama.Attributed;
using Replica.Core.Entity.Attachments;

namespace Replica.Core.Entity
{
    public class InMessage
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }

        [NotLogged]
        public string Text { get; set; }

        public UserInfo Sender { get; set; }
        public ChatInfo Chat { get; set; }

        [NotLogged]
        public InMessage Reply { get; set; }

        [NotLogged]
        public InMessage[] Forwarded { get; set; }

        [NotLogged]
        public Attachment[] Attachments { get; set; }
    }
}
