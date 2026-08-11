using System;
using System.Collections;
using System.Threading.Tasks;
using Coroutine;

namespace OperationSystem.Operations.Abstract
{
    public abstract partial class AbstractOperation
    {
        private bool _pendingComplete;
        private Task<bool>? _coroutineTask;
        private TaskCompletionSource<bool>? _taskCompletionSource;

        private void IncrementManual()
        {
            if (_coroutine == null || _taskCompletionSource == null)
                return;

            while (_coroutine.MoveNext())
            {
                if (_coroutine.InContinueState())
                    continue;

                return;
            }

            if (_coroutine.Exception == null)
                _taskCompletionSource.TrySetResult(true);
            else
                _taskCompletionSource.TrySetException(_coroutine.Exception);

            if (_pendingComplete)
                SetCompleted();

            _coroutine = null;
            _coroutineTask = null;
            _taskCompletionSource = null;
        }

        public Task RunStage(OperationStage stage)
        {
            if (_staging != OperationStaging.Manual)
                throw new InvalidOperationException();

            switch (stage)
            {
                case OperationStage.Initialization:
                    return CreateCoroutineTask(InitializationEnumerator());
                case OperationStage.Validate:
                    return CreateCoroutineTask(ValidateEnumerator());
                case OperationStage.TryAcquireLocks:
                    return CreateCoroutineTask(TryAcquireLocksEnumerator());
                case OperationStage.ReleaseLocks:
                    return CreateCoroutineTask(ReleaseLocksEnumerator());
                case OperationStage.RecordMutations:
                    return CreateCoroutineTask(RecordMutationsEnumerator());
                case OperationStage.Execute:
                    return CreateCoroutineTask(ExecuteEnumerator());
                case OperationStage.Complete:
                    _pendingComplete = true;
                    return CreateCoroutineTask(CompleteEnumerator());
                case OperationStage.Cancel:
                    _pendingComplete = true;
                    return CreateCoroutineTask(CancelEnumerator());
                case OperationStage.None:
                default:
                    throw new ArgumentOutOfRangeException(nameof(stage), stage, null);
            }
        }

        private async Task CreateCoroutineTask(IEnumerator enumerator)
        {
            if (_coroutineTask is { IsCompleted: false })
                await _coroutineTask;

            _taskCompletionSource = new TaskCompletionSource<bool>();
            _coroutineTask = _taskCompletionSource.Task;

            _coroutine = enumerator.ToCoroutine();

            await _coroutineTask;
        }
    }
}