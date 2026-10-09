using System;
using System.Threading;
using System.Threading.Tasks;
using SecondScratch.ThreadSafe.Operations;
using SecondScratch.ThreadSafe.Schedulers;

namespace SecondScratch.ThreadSafe.Tests.Mocks.Containers
{
    /*
    public class TransferContext
    {
        public Player Player { get; set; }
        public Container From { get; set; }
        public Container To { get; set; }
        public Item Item { get; set; }

        public bool AccessOk { get; set; }
    }

    public class TransferAccessException : Exception
    {
        public TransferAccessException(TransferContext cxt)
            : base($"'{cxt.Player}' has no access to move '{cxt.Item}' between '{cxt.From}' -> '{cxt.To}'.")
        {
        }
    }

    public class ValidateTransferStage : OperationStage<TransferContext>
    {
        public override string Name => "ValidateTransfer";

        public override ValueTask Execute(Transaction transaction, TransferContext context, CancellationToken ct)
        {
            context.AccessOk = context.Player.HasAccessTo(context.From) && context.Player.HasAccessTo(context.To);
            if (!context.AccessOk)
                throw new TransferAccessException(context);

            return new ValueTask(Task.CompletedTask);
        }
    }

    public class MoveItemStage : OperationStage<TransferContext>
    {
        public override string Name => "MoveItem";

        public override async ValueTask Execute(Transaction transaction, TransferContext context, CancellationToken ct)
        {
            var from = context.From;
            var to = context.To;
            var item = context.Item;

            await transaction.Apply(
                mutation: async () => await from.CreateRemoveCommand(item).Schedule(),
                rollback: async () => await from.CreateAddCommand(item).Schedule());

            await transaction.Apply(
                mutation: async () => await to.CreateAddCommand(item).Schedule(),
                rollback: async () => await to.CreateRemoveCommand(item).Schedule());
        }
    }

    public static class TransferOperation
    {
        public static OperationStage<TransferContext>[] CreateStages() =>
            new OperationStage<TransferContext>[]
            {
                new ValidateTransferStage(),
                new MoveItemStage()
            };

        public static Operation<TransferContext> Create(CancellationToken ct) => new(CreateStages(), ct);
    }
    */
}