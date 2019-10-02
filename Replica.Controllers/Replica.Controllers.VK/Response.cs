using System.Collections.Generic;
using Replica.Core.Messages;
using VkNet.Enums.Filters;

namespace Replica.Controllers.VK
{
    internal class Response
    {
        public List<VkNet.Model.Message> Updates { get; set; }
    }
}
