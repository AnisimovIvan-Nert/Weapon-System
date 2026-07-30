using OperationSystem.Component;

namespace OperationSystem.Containers.Components
{
    public interface IAccessLevel : IComponent
    {
        int Level { get; }
    }
    
    public struct AccessLevel : IAccessLevel
    {
        public int Level { get; }
        
        public AccessLevel(int level)
        {
            Level = level;
        }
    }
}