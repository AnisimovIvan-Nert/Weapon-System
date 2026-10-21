using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using SecondScratch.ThreadSafe.Operations.Common;

namespace SecondScratch.ThreadSafe.Tests.Mocks.Extensions
{
    public static class OperationExtensions
    {
        public static async ValueTask<List<ExceptionDispatchInfo>> WhenAll(this IEnumerable<IOperationHandler> operations)
        {
            var tasks = operations.Select(o => o.Execute()).ToArray();

            var exceptions = new List<ExceptionDispatchInfo>();
            foreach (var task in tasks)
            {
                try
                {
                    await task;
                }
                catch (Exception e)
                {
                    var info = ExceptionDispatchInfo.Capture(e);
                    exceptions.Add(info);
                }
            }

            return exceptions;
        }
    }
}