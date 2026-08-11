using System;
using System.Linq;
using OperationSystem.Component;

namespace OperationSystem.Units
{
    public static class UnitExtensions
    {
        public static Unit GetChild<T>(this Unit unit)
            where T : struct, IComponent
        {
            return unit.TryGetChild<T>() ?? throw new InvalidOperationException();
        }

        public static Unit? TryGetChild<T>(this Unit unit)
            where T : struct, IComponent
        {
            var result = unit.Children.FirstOrDefault(child => child.ComponentMask.Contains<T>());
            return result == default ? null : result;
        }

        public static T GetComponent<T>(this Unit unit, UnitWorld world)
            where T : struct, IComponent
        {
            return world.GetComponentArray<T>().GetComponent(unit.Id);
        }
        
        public static bool TryGetComponent<T>(this Unit unit, UnitWorld world, out T component)
            where T : IComponent
        {
            component = default!;
            var componentArray = world.TryGetComponentArray<T>(unit);
            if (componentArray == null)
                return false;
            
            component = componentArray.GetComponent<T>(unit.Id);
            return true;
        }

        public static void SetComponent<T>(this Unit unit, T component, UnitWorld world)
            where T : struct, IComponent
        {
            world.GetComponentArray<T>().SetComponent(unit.Id, component);
        }
        
        public static bool HasComponent<T>(this Unit unit, UnitWorld world)
            where T : struct, IComponent
        {
            return world.GetComponentArray<T>().HasComponent(unit);
        }
    }
}