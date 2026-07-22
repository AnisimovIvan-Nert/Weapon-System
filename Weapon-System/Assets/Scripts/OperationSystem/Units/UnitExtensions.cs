using System;
using System.Collections.Generic;
using System.Linq;

namespace OperationSystem.Units
{
    public class UnitFindException : Exception
    {
    }
    
    public static class UnitExtensions
    {
        public static T? TryFind<T>(this IUnit unit, bool deep = false)
            where T : IUnit
        {
            var queue = new Queue<IUnit>();
            queue.Enqueue(unit);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var result = current.Children.OfType<T>().FirstOrDefault();
                if (result != null)
                    return result;
                
                if (!deep)
                    break;

                foreach (var child in current.Children)
                    queue.Enqueue(child);
            }

            return default;
        }
        
        public static T Find<T>(this IUnit unit, bool deep = false)
            where T : IUnit
        {
            return unit.TryFind<T>(deep) ?? throw new UnitFindException();
        }
    }
}