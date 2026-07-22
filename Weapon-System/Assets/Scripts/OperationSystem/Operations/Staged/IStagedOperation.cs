using System;
using System.Threading.Tasks;

namespace OperationSystem.Operations.Staged
{
    public interface IStagedOperation : IOperation
    {
        Task Validate();

        Task TryAcquireLocks();
        Task ReleaseLocks();

        Task RecordPossibleMutations();

        Task Execute();

        Task Complete();
        Task Cancel(Exception exception);
    }
}