using System;
using Replica.Core.Entity.Attachments;

namespace Replica.Core.Entity
{
    public class OutMessage
    {
        public string Text { get; set; }
        public MessageFlags Flags { get; set; }
        public Button[] Buttons { get; set; }
        public Keyboard Keyboard { get; set; }
        public Attachment[] Attachments { get; set; }

        public static OutMessage FromCode(string code)
            => new OutMessage { Text = code, Flags = MessageFlags.Code };

        public static OutMessage FromText(string message)
            => new OutMessage { Text = message };
    }
}
