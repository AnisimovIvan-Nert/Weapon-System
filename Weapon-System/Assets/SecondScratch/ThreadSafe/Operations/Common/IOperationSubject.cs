using System.Threading.Tasks;

namespace SecondScratch.ThreadSafe.Operations.Common
{
    public interface IOperationSubject
    {
        ValueTask UpstreamOperation(IOperation operation);
        ValueTask DownstreamOperation(IOperation operation);
    }
}