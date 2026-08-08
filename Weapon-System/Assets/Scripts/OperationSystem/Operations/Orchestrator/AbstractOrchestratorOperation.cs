using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Coroutine;
using Coroutine.Instructions;
using OperationSystem.Operations.Data;
using OperationSystem.Operations.Middleware;
using OperationSystem.Operations.Staged;

namespace OperationSystem.Operations.Orchestrator
{
    public abstract class AbstractOrchestratorOperation : AbstractOperation
    {
        private ICollection<IStagedOperation>? _operations;
        private Task[]? _operationTasks;
        private bool _waitingOrchestratedOperations;

        private ICollection<IStagedOperation> Operations => _operations ?? throw new InvalidOperationException();
        private Task[] OperationTasks => _operationTasks ?? throw new InvalidOperationException();

        protected AbstractOrchestratorOperation(
            OperationIdentifier identifier, 
            IOperationMiddleware[] middlewares, 
            params IOperationData[] data) 
            : base(identifier, middlewares, data)
        {
        }

        public override void Increment(IOperationContext context)
        {
            if (!_waitingOrchestratedOperations)
            {
                Coroutine ??= IncrementEnumerator(context).ToCoroutine();

                while (Coroutine.MoveNext())
                {
                    if (Coroutine.InContinueState())
                        continue;

                    return;
                }
            
                AppendException(Coroutine.Exception);

                for (var i = 0; i < Operations.Count; i++)
                {
                    var operation = Operations.ElementAt(i);
                    
                    if (Exception == null)
                        OperationTasks[i] = operation.Complete();
                    else
                        OperationTasks[i] = operation.Cancel(Exception);
                }
                
                _waitingOrchestratedOperations = true;
            }

            var failedTask = OperationTasks.FirstOrDefault(task => task.IsFaulted);
            if (failedTask is { Exception: not null })
                ExceptionDispatchInfo.Capture(failedTask.Exception).Throw();

            if (Operations.Any(operation => !operation.IsCompleted))
                return;

            IsCompleted = true;
        }

        protected abstract ICollection<IStagedOperation> CreateAndRunOrchestratedOperations();
        
        protected override IEnumerator IncrementEnumerator(IOperationContext context)
        {
            _operations = CreateAndRunOrchestratedOperations();
            _operationTasks = new Task[_operations.Count];
            
            yield return Validate();
            yield return AcquireLocks();
            yield return RecordPossibleMutations();
            yield return Execute();
        }
        
        protected virtual IEnumerator Validate()
        {
            foreach (var operation in Operations)
                yield return new WaitForTask(operation.Validate());
        }
        
        protected virtual IEnumerator AcquireLocks()
        {
            var timeout = AcquireLocksTimeout;
            while (timeout-- > 0)
            {
                var success = false;
                yield return TryAcquireLocks().GetResult<bool>(result => success |= result);
                if (success)
                    yield break;

                yield return ReleaseResources();
                yield return null;
            }

            throw new AcquireException();
        }
        
        protected virtual IEnumerator RecordPossibleMutations()
        {
            foreach (var operation in Operations)
                yield return new WaitForTask(operation.RecordPossibleMutations());
        }
        
        protected virtual IEnumerator Execute()
        {
            foreach (var operation in Operations)
                yield return new WaitForTask(operation.Execute());
        }
        
        protected virtual IEnumerator ReleaseResources()
        {
            foreach (var operation in Operations)
                yield return new WaitForTask(operation.ReleaseLocks());
        }
        
        protected virtual IEnumerator TryAcquireLocks()
        {
            var result = true;
            foreach (var operation in Operations)
            {
                yield return HandleTaskResult(operation.TryAcquireLocks())
                    .GetResult<bool>(o => result &= o);
                    
                if (!result)
                    break;
            }

            yield return result;
            yield break;

            IEnumerator HandleTaskResult(Task task)
            {
                var catchException = new CatchException(new WaitForTask(task));
                yield return catchException;

                if (catchException.Exception is AcquireException)
                {
                    yield return false;
                    yield break;
                }

                if (catchException.Exception != null)
                    throw catchException.Exception;

                yield return true;
            }
        }
    }
}