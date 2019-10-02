using Replica.Core.Contexts;

namespace Replica.Core.Views
{
    public interface IView
    {
        void Enter(Context context, params object[] args);
        IView Clone();
        string Name { get; }
    }
}
