using System;
using System.Threading;
using System.Threading.Tasks;

namespace Scratch.InteractionArchitecture.Containers
{
    /// <summary>
    /// Shared context for a single "move an item from one container to another"
    /// by a single player.
    /// </summary>
    public sealed class TransferContext
    {
        public Player Player { get; set; }
        public Container From { get; set; }
        public Container To { get; set; }
        public InventorySlot Slot { get; set; }

        public bool AccessOk { get; set; }
        public bool CapacityOk { get; set; }
        public bool PresentOk { get; set; }
    }

    /// <summary>
    /// Stage 1 — validate every precondition atomically on the source's owner
    /// thread. Item presence is checked via <see cref="Container.RunOnOwnerAsync{T}(Func{T})"/>,
    /// target capacity via <see cref="Container.RunOnOwnerAsync{T}(Func{T})"/> on the target's
    /// owner thread. Access is a pure function and needs no marshalling.
    ///
    /// Any failure throws, which rolls back the whole interaction cleanly.
    /// </summary>
    public sealed class ValidateTransferStage : InteractionStage<TransferContext>
    {
        public override string Name => "ValidateTransfer";

        public override async Task ExecuteAsync(
            Transaction transaction,
            TransferContext context,
            Func<Task> yield,
            CancellationToken ct)
        {
            // Access check is pure and needs no thread marshalling.
            context.AccessOk = context.Player.HasAccessTo(context.From)
                            && context.Player.HasAccessTo(context.To);
            if (!context.AccessOk)
                throw new InvalidOperationException(
                    $"'{context.Player}' has no access to move '{context.Slot.Item}' " +
                    $"between '{context.From}' -> '{context.To}'.");

            // Marshal onto the correct owner threads for state-dependent checks.
            context.PresentOk = await context.From.RunOnOwnerAsync(
                () => context.From.Contains(context.Slot));
            if (!context.PresentOk)
                throw new InvalidOperationException(
                    $"'{context.Slot.Item}' is not present in '{context.From}'.");

            context.CapacityOk = await context.To.RunOnOwnerAsync(
                () => context.Slot.Item.Size <= context.To.FreeCapacity);
            if (!context.CapacityOk)
                throw new InvalidOperationException(
                    $"'{context.To}' cannot fit '{context.Slot.Item}' " +
                    $"(needs {context.Slot.Item.Size}, free {context.To.FreeCapacity}).");
        }
    }

    /// <summary>
    /// Stage 2 — atomically move the item, marshalling every container mutation
    /// onto its owner thread.
    ///
    /// The removal runs on the source owner thread; the addition asks the target
    /// to run on its own owner thread.  Order is always From-then-To, and
    /// compensations always undo To-then-From, keeping acquisition consistent so
    /// two transfers can never deadlock.
    ///
    /// Each mutation is registered on the transaction with an asynchronous
    /// compensation so a later failure or cancellation restores the item exactly
    /// where it started.
    /// </summary>
    public sealed class MoveItemStage : InteractionStage<TransferContext>
    {
        public override string Name => "MoveItem";

        public override async Task ExecuteAsync(
            Transaction transaction,
            TransferContext context,
            Func<Task> yield,
            CancellationToken ct)
        {
            var from = context.From;
            var to = context.To;
            var slot = context.Slot;

            // Re-validate presence on the source owner thread and capacity on the
            // target owner thread right before mutating, because another transfer
            // may have moved the item or filled the target since Stage 1.
            await from.RunOnOwnerAsync(() =>
            {
                if (!from.Contains(slot))
                    throw new InvalidOperationException(
                        $"'{slot.Item}' disappeared from '{from}' before the move.");
            });

            await to.RunOnOwnerAsync(() =>
            {
                if (slot.Item.Size > to.FreeCapacity)
                    throw new InvalidOperationException(
                        $"'{to}' no longer has room for '{slot.Item}'.");
            });

            // Remove from source on its owner thread.
            await transaction.ApplyAsync(
                mutation: () => from.RunOnOwnerAsync(() =>
                {
                    if (!from.Remove(slot))
                        throw new InvalidOperationException(
                            $"Failed to remove '{slot.Item}' from '{from}'.");
                    return true;
                }),
                compensationFactory: _ => from.RunOnOwnerAsync(() => from.Add(slot)));

            // Add to target on its owner thread.
            await transaction.ApplyAsync(
                mutation: () => to.RunOnOwnerAsync(() =>
                {
                    to.Add(slot); // throws if it no longer fits
                    return true;
                }),
                compensationFactory: _ => to.RunOnOwnerAsync(() => to.Remove(slot)));
        }
    }

    /// <summary>
    /// Convenience factory that builds a full transfer interaction.
    /// </summary>
    public static class TransferInteraction
    {
        public static InteractionStage<TransferContext>[] CreateStages() =>
            new InteractionStage<TransferContext>[]
            {
                new ValidateTransferStage(),
                new MoveItemStage()
            };
    }
}