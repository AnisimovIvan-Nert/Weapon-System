using System.Collections;

namespace Coroutine
{
    public static class EnumeratorExtensions
    {
        public static YieldCoroutine ToCoroutine(this IEnumerator enumerator) => new(enumerator);
    }
}