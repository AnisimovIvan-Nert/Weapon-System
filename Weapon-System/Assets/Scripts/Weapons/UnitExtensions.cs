using System.Collections.Generic;
using System.Linq;

namespace Weapons
{
    public static class UnitExtensions
    {
        public static T? TryFind<T>(this IUnit unit)
            where T : IUnit
        {
            IUnit? current;
            
            var queue = new Queue<IUnit>();
            queue.Enqueue(unit);
            while (queue.Count > 0)
            {
                current = queue.Dequeue();
                var result = current.Children.OfType<T>().FirstOrDefault();
                if (result != null)
                    return result;

                foreach (var child in current.Children)
                    queue.Enqueue(child);
            }
            
            current = unit;

            while (current != null)
            {
                var result = current.Children.OfType<T>().FirstOrDefault();

                if (result != null)
                    return result;

                current = current.Parent;
            }

            return default;
        }
    }
}