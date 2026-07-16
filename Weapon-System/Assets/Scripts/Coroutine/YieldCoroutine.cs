using System;
using System.Collections;
using Coroutine.Instructions;

namespace Coroutine
{
    public class YieldCoroutine : IEnumerator
    {
        private readonly IEnumerator _coroutine;
        
        private bool _moveNext = true;

        public Exception? Exception { get; private set; }
        public object? Current { get; private set; }

        public YieldCoroutine(IEnumerator coroutine)
        {
            _coroutine = coroutine;
        }

        public bool MoveNext()
        {
            try
            {
                if (!_moveNext)
                    return false;
            
                if (_coroutine.Current is IYieldInstruction yieldInstruction && !yieldInstruction.IsDone())
                    return true;

                if (_coroutine.Current is IEnumerator enumerator)
                {
                    if (enumerator.Current is IYieldInstruction innerYield && !innerYield.IsDone())
                        return true;

                    if (enumerator.MoveNext())
                    {
                        Current = enumerator.Current;
                        return true;
                    }
                }
            
                _moveNext = _coroutine.MoveNext();
                Current = _coroutine.Current;
                return _moveNext;
            }
            catch (Exception e)
            {
                Exception = e;
                _moveNext = false;
                return false;
            }
        }

        public void Reset() => throw new InvalidOperationException();

        public bool IsCompletedSuccessfully() => Exception == null;
    }
}