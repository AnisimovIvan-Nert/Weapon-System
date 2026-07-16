using System.Collections;
using Coroutine.Instructions;

namespace Coroutine
{
    public class YieldCoroutine
    {
        private readonly IEnumerator _coroutine;
        private bool _moveNext = true;

        public YieldCoroutine(IEnumerator coroutine)
        {
            _coroutine = coroutine;
        }

        public bool Yield()
        {
            if (!_moveNext)
                return false;
            
            if (_coroutine.Current is IYieldInstruction yieldInstruction && !yieldInstruction.IsDone())
                return true;
            
            _moveNext = _coroutine.MoveNext();
            return _moveNext;
        }
    }
}