using Replica.Core.Contexts;

namespace Replica.Core.Commands
{
    public interface IRestriction
    {
        bool Check(Context ctx);
    }
}