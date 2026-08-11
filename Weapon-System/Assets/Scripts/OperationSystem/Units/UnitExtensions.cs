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

        public static T GetComponent<T>(this Unit unit)
            where T : struct, IComponent
        {
            return unit.World.GetComponents<T>().GetComponent(unit.Id);
        }

        public static void SetComponent<T>(this Unit unit, T component)
            where T : struct, IComponent
        {
            unit.World.GetComponents<T>().SetComponent(unit.Id, component);
        }
        
        public static bool HasComponent<T>(this Unit unit)
            where T : struct, IComponent
        {
            return unit.World.GetComponents<T>().HasComponent(unit);
        }
    }
}