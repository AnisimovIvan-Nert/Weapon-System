using NUnit.Framework;
using OperationSystem.Operations;
using OperationSystem.Operations.Result;

namespace OperationSystem.TestExtensions
{
    public static class OperationAssertExtensions
    {
        public static void AssertPass(this IOperation operation)
        {
            Assert.IsTrue(operation.IsCompleted);
            Assert.IsTrue(operation.IsCompletedSuccessfully);
            Assert.IsNull(operation.Exception);
        }
        
        public static void AssertFail(this IOperation operation)
        {
            Assert.IsTrue(operation.IsCompleted);
            Assert.IsFalse(operation.IsCompletedSuccessfully);
            Assert.IsNotNull(operation.Exception);
        }
        
        public static void AssertFail<T>(this IOperation operation)
            where T : OperationException
        {
            Assert.IsTrue(operation.IsCompleted);
            Assert.IsFalse(operation.IsCompletedSuccessfully);
            Assert.IsNotNull(operation.Exception);
            Assert.Throws<T>(() => throw operation.Exception!);
        }
    }
}