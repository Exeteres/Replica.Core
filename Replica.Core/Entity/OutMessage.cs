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

        // Discord compatibility
        public string AuthorName { get; set; }
        public string AuthorUrl { get; set; }
        public string AuthorIcon { get; set; }
        public string Footer { get; set; }

        public static OutMessage FromCode(string code)
            => new OutMessage { Text = code, Flags = MessageFlags.Code };

        public static OutMessage FromText(string message)
            => new OutMessage { Text = message };
    }
}
