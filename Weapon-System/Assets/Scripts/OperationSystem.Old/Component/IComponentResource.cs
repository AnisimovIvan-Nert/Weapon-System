using OperationSystem.Component.Types;
using OperationSystem.Resource;

namespace OperationSystem.Component
{
    public interface IComponentResource : IResource
    {
        ComponentType Type { get; }
        T Read<T>() where T : IComponent;
        void Write<T>(T component) where T : IComponent;
        void PullData();
        void PushData();
    }
}