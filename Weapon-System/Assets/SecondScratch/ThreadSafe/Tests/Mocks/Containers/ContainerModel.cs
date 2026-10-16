using System;
using SecondScratch.ThreadSafe.Operations;

namespace SecondScratch.ThreadSafe.Tests.Mocks.Containers
{
    public class Item : AbstractOperationSubject
    {
        public int Size { get; }

        public Item(int size)
        {
            Size = size;
        }
    }

    public class Container : AbstractOperationSubject, IContainer
    {
        public int Capacity { get; }
        public int? OwnerId { get; }

        public int UsedCapacity { get; private set; }

        public int FreeCapacity => Capacity - UsedCapacity;

        public Container(int capacity, int? ownerId = null)
        {
            Capacity = capacity;
            OwnerId = ownerId;
        }

        public void Add(Item item)
        {
            var exceedCapacityMessage = $"'{nameof(Container)} has {FreeCapacity} free, item needs {item.Size}.";
            if (item.Size > FreeCapacity)
                throw new InvalidOperationException(exceedCapacityMessage);

            AddChild(item);
            UsedCapacity += item.Size;
        }

        public void Remove(Item item)
        {
            RemoveChild(item);
            UsedCapacity -= item.Size;
        }
    }

    public interface IContainer
    {
        int? OwnerId { get; }

        bool HasAccess(int id) => OwnerId == null || OwnerId.Equals(id);
    }

    public class Player
    {
        public int Id { get; }

        public Player(int id)
        {
            Id = id;
        }

        public bool HasAccessTo(IContainer container) => container.HasAccess(Id);
    }
}