using System.Linq;
using OperationSystem.Units;

namespace OperationSystem.Containers.Units
{
    public interface IAccessLevel : IUnit
    {
        int Level { get; }
    }
    
    public class AccessLevel
        : AbstractUnit
        , IAccessLevel
    {
        public int Level { get; }
        
        public AccessLevel(int level) : base(Enumerable.Empty<IUnit>())
        {
            Level = level;
        }
    }
}