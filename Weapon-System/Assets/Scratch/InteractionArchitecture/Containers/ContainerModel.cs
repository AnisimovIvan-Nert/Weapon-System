using System;
using System.Collections.Generic;
using System.Threading;

namespace Scratch.InteractionArchitecture.Containers
{
    public sealed class Item
    {
        public int Id { get; }
        public string Name { get; }

        // How much capacity the item occupies in a Container when carried.
        public int Size { get; }

        public Item(int id, string name, int size)
        {
            Id = id;
            Name = name;
            Size = size;
        }

        public override string ToString() => $"{Name}(#{Id})";
    }

    public sealed class InventorySlot
    {
        public Item Item { get; }
        public Container Owner { get; }

        public InventorySlot(Item item, Container owner)
        {
            Item = item;
            Owner = owner;
        }
    }

    /// <summary>
    /// A container that holds items up to a fixed capacity and records who owns
    /// it for permission checks.
    ///
    /// THREAD AFFINITY: every container is owned by exactly one thread — either
    /// the main thread (Owner == null) or a dedicated
    /// <see cref="ThreadDispatcher.WorkerThread"/>. All access to the container
    /// must happen on its owner thread; this is what makes it safe without
    /// locks, because each container is only ever touched by a single thread.
    ///
    /// Use <see cref="RunOnOwner"/> to marshal an operation onto the owner
    /// thread from any other thread.
    /// </summary>
    public sealed class Container
    {
        private readonly List<InventorySlot> _slots = new();
        private readonly ThreadDispatcher.WorkerThread _owner;

        public ThreadDispatcher.WorkerThread Owner => _owner;

        public int Id { get; }
        public string Name { get; }
        public int Capacity { get; }
        public object OwnerId { get; }

        public IReadOnlyList<InventorySlot> Slots => _slots;
        public int UsedCapacity { get; private set; }

        /// <param name="ownerThread">
        /// The thread that owns this container, or null to own it on the main
        /// thread (the caller's thread at construction time).
        /// </param>
        public Container(int id, string name, int capacity, object ownerId = null,
            ThreadDispatcher.WorkerThread ownerThread = null)
        {
            Id = id;
            Name = name;
            Capacity = capacity;
            OwnerId = ownerId;
            _owner = ownerThread;
        }

        public int FreeCapacity => Capacity - UsedCapacity;

        /// <summary>Marshals an operation onto this container's owner thread.
        /// If the caller is already on the owner thread it runs inline.</summary>
        public T RunOnOwner<T>(Func<T> func)
        {
            if (_owner == null)
            {
                AssertMainThread();
                return func();
            }
            return _owner.InvokeSync(func);
        }

        public void RunOnOwner(Action action) => RunOnOwner<object>(() => { action(); return null; });

        /// <summary>Adds an item. Must be called on the owner thread.</summary>
        public void Add(InventorySlot slot)
        {
            AssertOnOwner();
            if (slot.Item.Size > FreeCapacity)
                throw new InvalidOperationException(
                    $"'{Name}' has {FreeCapacity} free, item needs {slot.Item.Size}.");

            _slots.Add(slot);
            UsedCapacity += slot.Item.Size;
        }

        /// <summary>Removes an item. Must be called on the owner thread.</summary>
        public bool Remove(InventorySlot slot)
        {
            AssertOnOwner();
            if (!_slots.Remove(slot))
                return false;

            UsedCapacity -= slot.Item.Size;
            return true;
        }

        public bool Contains(InventorySlot slot)
        {
            AssertOnOwner();
            return _slots.Contains(slot);
        }

        private void AssertOnOwner()
        {
            if (_owner != null && Thread.CurrentThread.ManagedThreadId != _owner.ThreadId)
                throw new InvalidOperationException(
                    $"Container '{Name}' must be accessed on its owner thread " +
                    $"#{_owner.ThreadId}, but was touched on #{Thread.CurrentThread.ManagedThreadId}. " +
                    "Use RunOnOwner to marshal access.");
        }

        private void AssertMainThread()
        {
            if (_owner != null)
                throw new InvalidOperationException($"Container '{Name}' has a worker owner.");
            // Main-thread containers run on the scheduler (main) thread;
            // this is a defense-in-depth check against accidental cross-thread use.
        }

        public override string ToString() =>
            $"{Name}[{UsedCapacity}/{Capacity}](#{Id})" +
            (_owner != null ? $"@{_owner.Name}" : "@Main");
    }

    public sealed class Player
    {
        public int Id { get; }
        public string Name { get; }
        public Container Inventory { get; }

        public Player(int id, string name, Container inventory)
        {
            Id = id;
            Name = name;
            Inventory = inventory;
        }

        public bool HasAccessTo(Container container) =>
            ReferenceEquals(container, Inventory) ||
            container.OwnerId == null ||
            container.OwnerId.Equals(Id);

        public override string ToString() => Name;
    }
}
