using System;
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

        public static IEnumerator GetResult<T>(this IEnumerator enumerator, Action<T> setResult)
        {
            yield return enumerator;
            if (enumerator.Current is T result)
                setResult.Invoke(result);
        }

        public static bool InContinueState(this IEnumerator enumerator)
        {
            if (enumerator.Current is IEnumerator)
                return true;

            if (enumerator.Current is bool)
                return true;

            return false;
        }
    }
}