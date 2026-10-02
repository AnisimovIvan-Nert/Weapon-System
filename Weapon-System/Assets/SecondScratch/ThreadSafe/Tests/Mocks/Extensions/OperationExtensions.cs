using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SecondScratch.ThreadSafe.Operations;
using UnityEngine;

namespace SecondScratch.ThreadSafe.Tests.Mocks.Extensions
{
    public static class OperationExtensions
    {
        public static async ValueTask<int> WhenAll(this IEnumerable<IOperation> operations)
        {
            var tasks = operations.Select(o => o.Execute());

            var exceptions = 0;
            foreach (var task in tasks)
            {
                try
                {
                    await task;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    exceptions++;
                }
            }

            return exceptions;
        }
    }
}