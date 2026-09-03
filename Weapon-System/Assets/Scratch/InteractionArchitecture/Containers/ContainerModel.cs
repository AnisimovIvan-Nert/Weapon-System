using System;
using System.Collections.Generic;

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
    /// A container that holds items up to a fixed capacity and records
    /// who it is owned by for permission checks.
    /// </summary>
    public sealed class Container
    {
        private readonly List<InventorySlot> _slots = new();

        public int Id { get; }
        public string Name { get; }
        public int Capacity { get; }
        public object OwnerId { get; }

        public IReadOnlyList<InventorySlot> Slots => _slots;
        public int UsedCapacity { get; private set; }

        public Container(int id, string name, int capacity, object ownerId = null)
        {
            Id = id;
            Name = name;
            Capacity = capacity;
            OwnerId = ownerId;
        }

        public int FreeCapacity => Capacity - UsedCapacity;

        /// <summary>Adds an item. Caller must have validated capacity beforehand.</summary>
        public void Add(InventorySlot slot)
        {
            if (slot.Item.Size > FreeCapacity)
                throw new InvalidOperationException(
                    $"'{Name}' has {FreeCapacity} free, item needs {slot.Item.Size}.");

            _slots.Add(slot);
            UsedCapacity += slot.Item.Size;
        }

        /// <summary>Removes an item. Caller must have validated it is present.</summary>
        public bool Remove(InventorySlot slot)
        {
            if (!_slots.Remove(slot))
                return false;

            UsedCapacity -= slot.Item.Size;
            return true;
        }

        public bool Contains(InventorySlot slot) => _slots.Contains(slot);

        public override string ToString() => $"{Name}[{UsedCapacity}/{Capacity}](#{Id})";
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
