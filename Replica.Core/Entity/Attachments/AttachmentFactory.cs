namespace Replica.Core.Entity.Attachments
{
    public class AttachmentFactory
    {
        private readonly string _controller;

        public AttachmentFactory(string controller)
        {
            _controller = controller;
        }

        public Photo CreatePhoto(PhotoSize[] sizes)
        {
            return new Photo(_controller, sizes);
        }

        public Document CreateDocument(string id, string src, string name, int size)
        {
            return new Document(_controller, id, src, name, size);
        }

        public PhotoSize CreatePhotoSize(string id, string src, int width, int height, int size)
        {
            return new PhotoSize(_controller, id, src, width, height, size);
        }

        public Sticker CreateSticker(string id, string src, int width, int height, int size, string emoji)
        {
            return new Sticker(_controller, id, src, width, height, size, emoji);
        }

        public Voice CreateVoice(string id, string src, int duration, int size)
        {
            return new Voice(_controller, id, src, duration, size);
        }

        public WebPage CreateWebPage(string id, string src)
        {
            return new WebPage(_controller, id, src);
        }
    }
}