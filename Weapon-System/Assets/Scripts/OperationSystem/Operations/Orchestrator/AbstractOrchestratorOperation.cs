using System;
using System.Collections;
using System.Collections.Generic;
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

        private ICollection<IStagedOperation> Operations => _operations ?? throw new InvalidOperationException();

        protected AbstractOrchestratorOperation(
            Guid identifier, 
            IEnumerable<IOperationMiddleware> middlewares, 
            params IOperationData[] data) 
            : base(identifier, middlewares, data)
        {
        }

        public override void Increment(IOperationContext context)
        {
            _operations ??= GetOrchestratedOperations();
            
            Coroutine ??= IncrementEnumerator(context).ToCoroutine();

            while (Coroutine.MoveNext())
            {
                if (Coroutine.InContinueState())
                    continue;

                return;
            }

            IsCompleted = true;
            AppendException(Coroutine.Exception);

            foreach (var operation in Operations)
            {
                if (Exception == null)
                    operation.Complete();
                else
                    operation.Cancel(Exception);
            }
        }

        protected abstract ICollection<IStagedOperation> GetOrchestratedOperations();
        protected abstract void RunOrchestratedOperations(ICollection<IStagedOperation> operations);
        
        protected override IEnumerator IncrementEnumerator(IOperationContext context)
        {
            RunOrchestratedOperations(Operations);

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