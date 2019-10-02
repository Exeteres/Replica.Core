namespace Replica.Core.Entity.Attachments
{
    public class PhotoSize : Attachment
    {
        public int Width { get; }
        public int Height { get; }
        public int FileSize { get; }

        public PhotoSize(string controller, string id, string src, int width, int height, int size) : base(controller, id, src)
        {
            Width = width;
            Height = height;
            FileSize = size;
        }
    }
}