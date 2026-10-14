using System;
using System.Linq;
using System.Threading.Tasks;

namespace SecondScratch.Shared
{
    public static class ValueTaskExtensions
    {
        public static async ValueTask<T> WhenAny<T>(this ValueTask<T>[] tasks)
        {
            if (tasks.Length == 0)
                throw new InvalidOperationException();

            foreach (var task in tasks)
            {
                if (task.IsCompleted)
                    return await task;
            }

            return await await Task.WhenAny(tasks.Select(o => o.AsTask()));
        }
    }
}