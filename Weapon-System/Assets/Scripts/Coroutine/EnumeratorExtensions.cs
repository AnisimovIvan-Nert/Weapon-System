using System.Collections;

namespace Coroutine
{
    public static class EnumeratorExtensions
    {
        public static YieldCoroutine ToCoroutine(IEnumerator enumerator) => new YieldCoroutine(enumerator);
    }
}