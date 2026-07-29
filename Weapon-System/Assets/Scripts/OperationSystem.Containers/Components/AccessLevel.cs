using OperationSystem.Component;

namespace OperationSystem.Containers.Components
{
    public interface IAccessLevel : IComponent
    {
        int Level { get; }
    }
    
    public class AccessLevel
        : AbstractComponent
        , IAccessLevel
    {
        public int Level { get; }
        
        public AccessLevel(int level)
        {
            Level = level;
        }
    }
}