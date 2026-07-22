using System;
using System.Collections;
using System.Threading.Tasks;
using Coroutine;
using Coroutine.Instructions;
using OperationSystem.Containers.Units;
using OperationSystem.Containers.Units.Containers;
using OperationSystem.Handlers.Units;
using OperationSystem.Operations;
using OperationSystem.Units;

namespace OperationSystem.Containers.Operations
{
    public class MovingObjectOperation : AbstractOperation
    {
        private readonly IPlayer _executor;
        private readonly IUnit _target;
        
        private readonly IUnitOperationHandler<IContainer> _sender;
        private readonly IUnitOperationHandler<IContainer> _receiver;

        private SendObjectUnitOperation? _sendOperation;
        private ReceiveObjectOperation? _receiveObjectOperation;

        private SendObjectUnitOperation SendOperation => _sendOperation 
                                                         ?? throw new InvalidOperationException();

        private ReceiveObjectOperation ReceiveObjectOperation => _receiveObjectOperation 
                                                                 ?? throw new InvalidOperationException();
        
        public MovingObjectOperation(
            Guid identifier,
            IPlayer executor,
            IUnit target,
            IUnitOperationHandler<IContainer> sender,
            IUnitOperationHandler<IContainer> receiver) 
            : base(identifier)
        {
            _executor = executor;
            _target = target;
            _sender = sender;
            _receiver = receiver;
        }

        public override void Increment(IOperationContext context)
        {
            _sendOperation ??= new SendObjectUnitOperation(Identifier, _executor, _target);
            _receiveObjectOperation ??= new ReceiveObjectOperation(Identifier, _executor, _target);
            
            Coroutine ??= IncrementEnumerator(context).ToCoroutine();

            while (Coroutine.MoveNext())
            {
                if (Coroutine.InContinueState())
                    continue;

                return;
            }

            IsCompleted = true;
            AppendException(Coroutine.Exception);

            if (Exception == null)
            {
                _sendOperation.Complete();
                _receiveObjectOperation.Complete();
            }
            else
            {
                _sendOperation.Cancel(Exception);
                _receiveObjectOperation.Cancel(Exception);
            }
        }

        protected override IEnumerator IncrementEnumerator(IOperationContext context)
        {
            SendOperation.RunOperation(_sender);
            ReceiveObjectOperation.RunOperation(_receiver);

            yield return Validate();
            yield return AcquireLocks();
            yield return RecordPossibleMutations();
            yield return Execute();
        }

        private IEnumerator Validate()
        {
            yield return new WaitForTask(SendOperation.Validate());
            yield return new WaitForTask(ReceiveObjectOperation.Validate());
        }
        
        private IEnumerator AcquireLocks()
        {
            var timeout = AcquireLocksTimeout;

            while (timeout > 0)
            {
                timeout--;

                var success = false;
                yield return Acquire().GetResult<bool>(result => success |= result);
                
                if (success)
                    yield break;

                yield return new WaitForTask(SendOperation.ReleaseLocks());
                yield return new WaitForTask(ReceiveObjectOperation.ReleaseLocks());
            }

            throw new AcquireException();

            IEnumerator Acquire()
            {
                var result = true;
                yield return HandleTaskResult(SendOperation.TryAcquireLocks()).GetResult<bool>(o => result &= o);
                
                if (result)
                    yield return HandleTaskResult(ReceiveObjectOperation.TryAcquireLocks()).GetResult<bool>(o => result &= o);

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
        
        private IEnumerator RecordPossibleMutations()
        {
            yield return new WaitForTask(SendOperation.RecordPossibleMutations());
            yield return new WaitForTask(ReceiveObjectOperation.RecordPossibleMutations());
        }
        
        private IEnumerator Execute()
        {
            yield return new WaitForTask(SendOperation.Execute());
            yield return new WaitForTask(ReceiveObjectOperation.Execute());
        }
    }
}