namespace Replica.Core.Entity.Attachments
{
    public class Voice : Attachment
    {
        public int Duration { get; }
        public int FileSize { get; }

        public Voice(string controller, string id, string src, int duration, int size) : base(controller, id, src)
        {
            Duration = duration;
            FileSize = size;
        }
    }
}