using System;
using System.Collections.Generic;

namespace Scratch.InteractionArchitecture.Containers
{
    public interface IIdentifiable
    {
        int Id { get; }
    }
    
    public sealed class Item
    {
        public int Id { get; }
        public int Size { get; }

        public Item(int id, int size)
        {
            Id = id;
            Size = size;
        }

        public override string ToString() => $"{nameof(Item)}:{Id}";
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
    
    public sealed class Container : ThreadObject
    {
        private readonly List<InventorySlot> _slots = new();
        
        public int Capacity { get; }
        public int? OwnerId { get; }

        public IReadOnlyList<InventorySlot> Slots => _slots;
        public int UsedCapacity { get; private set; }
        
        public int FreeCapacity => Capacity - UsedCapacity;
        
        public Container(int id, int capacity, int? ownerId = null, ThreadDispatcher.WorkerThread ownerThread = null)
            : base (id, ownerThread)
        {
            Capacity = capacity;
            OwnerId = ownerId;
        }
        
        public void Add(InventorySlot slot)
        {
            AssertOnOwner();
            
            if (slot.Item.Size > FreeCapacity)
                throw new InvalidOperationException(
                    $"'{nameof(Container)}:{Id} has {FreeCapacity} free, item needs {slot.Item.Size}.");

            _slots.Add(slot);
            UsedCapacity += slot.Item.Size;
        }
        
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

        public override string ToString() => $"{nameof(Container)}:{Id}[{UsedCapacity}/{Capacity}](#{Id})" +
                                             base.ToString();
    }

    public sealed class Player : ThreadObject
    {
        public Container Inventory { get; }

        public Player(int id, Container inventory, ThreadDispatcher.WorkerThread ownerThread = null)
            : base(id, ownerThread)
        {
            Inventory = inventory;
        }

        public bool HasAccessTo(Container container) =>
            ReferenceEquals(container, Inventory) ||
            container.OwnerId == null ||
            container.OwnerId.Equals(Id);

        public override string ToString() => $"{nameof(Player)}:{Id}";
    }
}
