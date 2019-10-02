using Replica.Core.Contexts;

namespace Replica.Core.Handlers
{
    public interface IHandler
    {
        IHandler Clone();
        IHandler SetNext(IHandler next);
        void Process(Context context);
        Context Context { get; }
    }
}
