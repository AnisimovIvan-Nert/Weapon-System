using System.Collections;

namespace Coroutine
{
    public static class EnumeratorExtensions
    {
        public static YieldCoroutine ToCoroutine(this IEnumerator enumerator) => new(enumerator);

        public static void Wait(this IEnumerator enumerator, int? timeout = null)
        {
            while (enumerator.MoveNext() && timeout is null or > 0)
            {
            }
        }
    }
}