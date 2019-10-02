using System;
namespace Replica.Core.Views
{
    public class ViewAttribute : Attribute
    {
        public ViewInfo Info { get; }

        public ViewAttribute(string name)
        {
            Info = new ViewInfo { Name = name };
        }
    }
}