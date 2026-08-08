using System;
using System.Collections;
using System.Runtime.ExceptionServices;
using Coroutine.Instructions;

namespace Coroutine
{
    public delegate bool OnMoveNext(YieldCoroutine coroutine, bool moveNext);

    public class YieldCoroutine : IEnumerator
    {
        private readonly IEnumerator _coroutine;
        private readonly OnMoveNext? _onMoveNext;

        private bool _moveNext = true;
        private YieldCoroutine? _innerCoroutine;
        private bool _stopExceptionPropagation;

        public Exception? Exception { get; private set; }

        public object? Current => _innerCoroutine != null
            ? _innerCoroutine.Current
            : _coroutine.Current;

        public YieldCoroutine(IEnumerator coroutine, OnMoveNext? onMoveNext = null)
        {
            _coroutine = coroutine;
            _onMoveNext = onMoveNext;
        }

        public bool MoveNext()
        {
            var result = TryMoveNext();
            if (_onMoveNext != null)
                result = _onMoveNext.Invoke(this, result);
            return result;
        }

        public void Reset() => throw new InvalidOperationException();

        public bool IsCompletedSuccessfully() => Exception == null;

        public void StopExceptionPropagation()
        {
            _stopExceptionPropagation = true;
        }

        private bool TryMoveNext()
        {
            try
            {
                if (!_moveNext)
                    return false;

                if (_coroutine.Current is IYieldInstruction yieldInstruction && !yieldInstruction.IsDone())
                    return true;

                if (_innerCoroutine == null && _coroutine.Current is IEnumerator enumerator)
                    _innerCoroutine = enumerator.ToCoroutine();

                if (_innerCoroutine != null)
                {
                    if (_innerCoroutine.MoveNext())
                        return true;

                    if (_innerCoroutine.Exception != null && !_innerCoroutine._stopExceptionPropagation)
                        ExceptionDispatchInfo.Capture(_innerCoroutine.Exception).Throw();

                    _innerCoroutine = null;
                }

                _moveNext = _coroutine.MoveNext();
                return _moveNext;
            }
            catch (Exception e)
            {
                Exception = e;
                _moveNext = false;
                return false;
            }
        }
    }
}