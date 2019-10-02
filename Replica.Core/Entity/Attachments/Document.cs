namespace Replica.Core.Entity.Attachments
{
    public class Document : Attachment
    {
        public string FileName { get; }
        public int FileSize { get; }

        public Document(string controller, string id, string src, string name, int size) : base(controller, id, src)
        {
            FileName = name;
            FileSize = size;
        }
    }
}