using Replica.Core.Contexts;

namespace Replica.Core.Commands
{
    public interface IParameterRestriction
    {
        bool Check(Context context, object param);
    }
}