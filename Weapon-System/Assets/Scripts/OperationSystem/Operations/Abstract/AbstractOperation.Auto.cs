using System.Collections;
using Coroutine;

namespace OperationSystem.Operations.Abstract
{
    public abstract partial class AbstractOperation
    {
        private bool _inLastStage;
        
        private void IncrementAuto()
        {
            _coroutine ??= IncrementEnumerator().ToCoroutine();

            while (true)
            {
                switch (_coroutine.MoveNext())
                {
                    case false when _inLastStage:
                        IsCompleted = true;
                        return;
                    case false when !_inLastStage:
                    {
                        _inLastStage = true;
                    
                        if (_coroutine.IsCompletedSuccessfully())
                        {
                            _coroutine = CompleteEnumerator().ToCoroutine();
                        }
                        else
                        {
                            AppendException(_coroutine.Exception);
                            _coroutine = CancelEnumerator().ToCoroutine();
                        }
                    
                        continue;
                    }
                }

                if (_coroutine.InContinueState())
                    continue;

                return;
            }

            IEnumerator IncrementEnumerator()
            {
                yield return InitializationEnumerator();
                yield return ValidateEnumerator();
                yield return AcquireLocks();
                yield return RecordMutationsEnumerator();
                yield return ExecuteEnumerator();
            }

            IEnumerator AcquireLocks()
            {
                var timeout = AcquireLocksTimeout;
                while (timeout-- > 0)
                {
                    var coroutine = TryAcquireLocksEnumerator().CatchException<AcquireException>();
                    yield return coroutine;
                    if (coroutine.Exception is not AcquireException) 
                        yield break;
                    
                    yield return ReleaseLocksEnumerator();
                    yield return null;
                }

                throw new AcquireException();
            }
        }
    }
}