using System.Linq;
namespace Replica.Core.Entity.Attachments
{
    public class Photo : Attachment
    {
        public Photo(string controller, PhotoSize[] sizes) : base(controller, sizes.Last().FileId, sizes.Last().Source)
        {
            Sizes = sizes;
        }

        public PhotoSize[] Sizes { get; }
    }
}