using System.Reflection;
using System;
using Replica.Core.Contexts;

namespace Replica.Core.Flows
{
    public interface IFlow
    {
        void Enter(Context context, params object[] args);
        IFlow Clone();
        string Name { get; }
        void Process(string message, MethodInfo node);
        FlowInternalButton[] State { get; }
        void Rollback();
        void Leave(string message);
    }
}
