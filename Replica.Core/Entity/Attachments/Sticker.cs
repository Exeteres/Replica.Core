namespace Replica.Core.Entity.Attachments
{
    public class Sticker : Attachment
    {
        public int FileSize { get; }
        public int Width { get; }
        public int Height { get; }
        public string Emoji { get; }

        public Sticker(string controller, string src, string id, int fileSize, int width, int height, string emoji) : base(controller, src, id)
        {
            FileSize = fileSize;
            Width = width;
            Height = height;
            Emoji = emoji;
        }
    }
}