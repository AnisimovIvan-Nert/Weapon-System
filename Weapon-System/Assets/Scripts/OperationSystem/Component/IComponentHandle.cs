using OperationSystem.Component.Types;

namespace OperationSystem.Component
{
    public interface IComponentHandle
    {
        ComponentType Type { get; }

        T Read<T>() where T : IComponent;
        void Write<T>(T component) where T : IComponent;

        void PullData();
        void PushData();
    }
}