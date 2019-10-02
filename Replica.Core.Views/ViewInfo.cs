using System.Reflection;

namespace Replica.Core.Views
{
    public class ViewInfo
    {
        public string Name { get; set; }
        public MethodInfo[] Methods { get; set; }
        public MethodInfo Default { get; set; }
    }
}