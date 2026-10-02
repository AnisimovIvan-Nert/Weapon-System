using System;
using System.Collections.Generic;
using SecondScratch.ThreadSafe.Commands;

namespace SecondScratch.ThreadSafe.Tests.Mocks.Containers
{
    public class Item
    {
        public int Size { get; }

        public Item(int size)
        {
            Size = size;
        }
    }
    
    public partial class Container : IOwnedContainer
    {
        private readonly List<Item> _items = new();
        
        public int Capacity { get; }
        public int? OwnerId { get; }

        public IReadOnlyList<Item> Items => _items;
        public int UsedCapacity { get; private set; }

        public int FreeCapacity => Capacity - UsedCapacity;

        public Container(int capacity, int? ownerId = null)
        {
            Capacity = capacity;
            OwnerId = ownerId;
        }

        public void Add(Item item)
        {
            if (item.Size > FreeCapacity)
                throw new InvalidOperationException(
                    $"'{nameof(Container)} has {FreeCapacity} free, item needs {item.Size}.");

            _items.Add(item);
            UsedCapacity += item.Size;
        }

        public bool Remove(Item item)
        {
            if (!_items.Remove(item))
                return false;

            UsedCapacity -= item.Size;
            return true;
        }

        public bool Contains(Item item)
        {
            return _items.Contains(item);
        }
    }
    
    public partial class Container
    {
        public AddCommand CreateAddCommand(Item item) => new(this, item);
        public RemoveCommand CreateRemoveCommand(Item item) => new(this, item);
        
        public class AddCommand : BaseCommand<Container>
        {
            private readonly Item _item;

            public AddCommand(Container target, Item item)
                : base(target)
            {
                _item = item;
            }

            protected override void InnerExecute() => TypedTarget.Add(_item);
        }
        
        public class RemoveCommand : BaseCommand<Container>
        {
            private readonly Item _item;

            public RemoveCommand(Container target, Item item)
                : base(target)
            {
                _item = item;
            }

            protected override void InnerExecute() => TypedTarget.Remove(_item);
        }
    }

    public interface IOwnedContainer : IContainer
    {
        int? OwnerId { get; }

        bool HasAccess(int id) => OwnerId == null || OwnerId.Equals(id);
    }
    
    public interface IContainer { }

    public class Player
    {
        public int Id { get; }

        public Player(int id)
        {
            Id = id;
        }

        public bool HasAccessTo(IContainer container)
        {
            return container switch
            {
                IOwnedContainer ownedContainer => ownedContainer.HasAccess(Id),
                _ => false
            };
        }
    }
}