using System;
using System.Threading.Tasks;

namespace SecondScratch.ThreadSafe.Operations.Common
{
    public class OperationEnforceComplete : Exception
    {
        public bool Cascade { get; }
        
        public OperationEnforceComplete(bool cascade)
        {
            Cascade = cascade;
        }
    }
    
    public interface IOperationHandler : IOperation
    {
        ValueTask Execute();
        void Cancel();
    }
}