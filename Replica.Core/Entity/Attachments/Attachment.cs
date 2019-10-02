namespace Replica.Core.Entity.Attachments
{
    public class Attachment
    {
        public Attachment(string controller, string id, string src)
        {
            Controller = controller;
            FileId = id;
            Source = src;
        }

        public string FileId { get; set; }
        public string Source { get; set; }
        public string Controller { get; set; }
    }
}