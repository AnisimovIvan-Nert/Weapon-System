using SecondScratch.ThreadSafe.Operations.Common;

namespace SecondScratch.ThreadSafe.Operations
{
    public static class OperationDataExtensions
    {
        public static OperationData<TOut> WithContext<TIn, TOut>(this OperationData<TIn> data, TOut context)
            where TIn : IOperationContext
            where TOut : IOperationContext
        {
            return new OperationData<TOut>(data.Transaction, context, data.Cts);
        }
    }
}