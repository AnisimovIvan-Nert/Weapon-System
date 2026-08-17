using System;
using System.Linq;
using OperationSystem.Component;
using OperationSystem.Units.Child;

namespace OperationSystem.Units
{
    public static class UnitExtensions
    {
        public static bool TryAddChild(this Unit unit, Unit child)
        {
            return unit.Asset.TryAddChild(child.Asset);
        }
        
        public static bool TryRemoveChild(this Unit unit, Unit child)
        {
            return unit.Asset.TryRemoveChild(child.Asset);
        }
        
        public static Unit GetChild<T>(this Unit unit, UnitWorld world)
            where T : struct, IComponent
        {
            return unit.TryGetChild<T>(world) ?? throw new InvalidOperationException();
        }

        public static Unit? TryGetChild<T>(this Unit unit, UnitWorld world)
            where T : struct, IComponent
        {
            var children = unit.GetComponent<ChildrenComponent>(world);
            var result = children.Children.FirstOrDefault(child => child.ComponentMask.Contains<T>());
            return result == default ? null : result;
        }

        public static T GetComponent<T>(this Unit unit, UnitWorld world)
            where T : struct, IComponent
        {
            return world.GetComponentArray<T>().GetComponent(unit);
        }
        
        public static bool TryGetComponent<T>(this Unit unit, UnitWorld world, out T component)
            where T : IComponent
        {
            component = default!;
            var componentArray = world.TryGetComponentArray<T>(unit);
            if (componentArray == null)
                return false;
            
            component = componentArray.GetComponent<T>(unit);
            return true;
        }

        public static void SetComponent<T>(this Unit unit, T component, UnitWorld world)
            where T : struct, IComponent
        {
            world.GetComponentArray<T>().SetComponent(unit, component);
        }
        
        public static bool HasComponent<T>(this Unit unit, UnitWorld world)
            where T : struct, IComponent
        {
            return world.GetComponentArray<T>().HasComponent(unit);
        }
    }
}