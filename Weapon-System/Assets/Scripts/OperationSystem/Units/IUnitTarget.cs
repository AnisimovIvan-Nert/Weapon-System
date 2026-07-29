using System.Collections.Generic;
using OperationSystem.Component;

namespace OperationSystem.Units
{
    public interface IUnitTarget
    {
        IEnumerable<IComponentHandle> EnumerateComponents(UnitWorld unitWorld);
        
        T PullData<T>(T component, UnitWorld unitWorld) where T : IComponent;
        T PushData<T>(T component, UnitWorld unitWorld) where T : IComponent;
        
        bool IsDirty<T>(T component, UnitWorld unitWorld) where T : IComponent;
    }
}