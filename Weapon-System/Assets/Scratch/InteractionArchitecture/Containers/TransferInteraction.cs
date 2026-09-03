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
    /// Stage 1 — validate every precondition against the *current* state:
    /// the item is actually in the source container, it fits in the target's
    /// free space, and the player has access to both containers.
    ///
    /// Any failure throws, which rolls back the whole interaction (here there
    /// are no mutations yet, so a rollback is a no-op that just aborts cleanly).
    /// </summary>
    public sealed class ValidateTransferStage : InteractionStage<TransferContext>
    {
        public override string Name => "ValidateTransfer";

        public override Task ExecuteAsync(
            Transaction transaction,
            TransferContext context,
            Func<Task> yield,
            CancellationToken ct)
        {
            // Access to source and destination must be checked atomically.
            context.AccessOk = context.Player.HasAccessTo(context.From)
                            && context.Player.HasAccessTo(context.To);

            if (!context.AccessOk)
                throw new InvalidOperationException(
                    $"'{context.Player}' has no access to move '{context.Slot.Item}' " +
                    $"between '{context.From}' -> '{context.To}'.");

            // Item must currently be physically present in the source.
            context.PresentOk = context.From.Contains(context.Slot);
            if (!context.PresentOk)
                throw new InvalidOperationException(
                    $"'{context.Slot.Item}' is not present in '{context.From}'.");

            // Target must have free capacity for the item's size.
            context.CapacityOk = context.Slot.Item.Size <= context.To.FreeCapacity;
            if (!context.CapacityOk)
                throw new InvalidOperationException(
                    $"'{context.To}' cannot fit '{context.Slot.Item}' " +
                    $"(needs {context.Slot.Item.Size}, free {context.To.FreeCapacity}).");

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Stage 2 — atomically move the item: remove from source and add to target.
    ///
    /// Every mutation is registered on the transaction with a compensation, so
    /// if a *later* stage fails (or the interaction is cancelled mid-flight) the
    /// item is restored exactly where it started.
    ///
    /// Mutation methods are treated as the source of truth: if the state changed
    /// under us (e.g. another transfer removed the item first), the mutation
    /// throws and the interaction rolls back, preserving data integrity.
    /// </summary>
    public sealed class MoveItemStage : InteractionStage<TransferContext>
    {
        public override string Name => "MoveItem";

        public override Task ExecuteAsync(
            Transaction transaction,
            TransferContext context,
            Func<Task> yield,
            CancellationToken ct)
        {
            var from = context.From;
            var to = context.To;
            var slot = context.Slot;

            // Re-validate capacity and presence right before mutating, because
            // concurrent interactions may have changed the containers since
            // Stage 1.  If the state no longer matches, abort (throw -> rollback).
            if (!from.Contains(slot))
                throw new InvalidOperationException(
                    $"'{slot.Item}' disappeared from '{from}' before the move.");
            if (slot.Item.Size > to.FreeCapacity)
                throw new InvalidOperationException(
                    $"'{to}' no longer has room for '{slot.Item}'.");

            // Remove from source, add to target. Each mutation is compensated so
            // a later cancellation restores the original layout.
            transaction.Apply(
                mutation: () =>
                {
                    if (!from.Remove(slot))
                        throw new InvalidOperationException(
                            $"Failed to remove '{slot.Item}' from '{from}'.");
                    return true;
                },
                compensationFactory: _ => () =>
                {
                    // Undo the removal: put the slot back into the source.
                    from.Add(slot);
                });

            transaction.Apply(
                mutation: () =>
                {
                    to.Add(slot); // throws if it somehow no longer fits
                    return true;
                },
                compensationFactory: _ => () =>
                {
                    // Undo the addition: take it back out of the target.
                    to.Remove(slot);
                });

            return Task.CompletedTask;
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
