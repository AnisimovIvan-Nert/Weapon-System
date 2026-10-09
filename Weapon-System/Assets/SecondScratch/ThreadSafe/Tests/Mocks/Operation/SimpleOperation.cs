using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SecondScratch.ThreadSafe.Operations;
using SecondScratch.ThreadSafe.Operations.Common;

namespace SecondScratch.ThreadSafe.Tests.Mocks.Operation
{
    public class SimpleContext : IOperationContext
    {
        public int Counter;
        public int StatefulCounter;
        public int StatefulDiff = 10;

        public ValueTask Mutate()
        {
            Counter++;
            return new ValueTask(Task.CompletedTask);
        }

        public ValueTask Rollback()
        {
            Counter--;
            return new ValueTask(Task.CompletedTask);
        }

        public ValueTask<int> StatefulMutate()
        {
            StatefulCounter += StatefulDiff;
            return new ValueTask<int>(StatefulDiff);
        }

        public ValueTask StatefulRollback(int diff)
        {
            StatefulCounter -= diff;
            return new ValueTask(Task.CompletedTask);
        }
    }

    public class SimpleOperationException : Exception
    {
    }

    public class SimpleOperation : AbstractOperation<SimpleContext>
    {
        public bool MustThrow;
        public bool EnforceComplete;
        public bool Cascade;

        public SimpleOperation(
            IEnumerable<IOperationSubject> subjects,
            IEnumerable<OperationTypes> types,
            IEnumerable<OperationMember> members,
            bool mustThrow = false,
            bool enforceComplete = false,
            bool cascade = false)
            : base(subjects, types, members)
        {
            MustThrow = mustThrow;
            EnforceComplete = enforceComplete;
            Cascade = cascade;
        }

        protected override async ValueTask InnerExecute(Transaction transaction, SimpleContext context,
            CancellationToken ct)
        {
            if (EnforceComplete)
                throw new OperationEnforceComplete(Cascade);
            
            await transaction.Apply(context.Mutate, context.Rollback);
            await transaction.Apply(context.StatefulMutate, context.StatefulRollback);

            if (MustThrow)
                throw new SimpleOperationException();
        }
    }
}