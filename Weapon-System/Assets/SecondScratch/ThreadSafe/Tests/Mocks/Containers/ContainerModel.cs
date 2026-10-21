using System;
using System.Linq;
using System.Threading.Tasks;
using SecondScratch.ThreadSafe.Operations;
using SecondScratch.ThreadSafe.Operations.Common;

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

        public override async ValueTask UpstreamOperation<T>(
            IOperation operation,
            T context,
            OperationData data,
            OperationMember? member)
        {
            if (member == null || member.Value.Type is not Members.Source and not Members.Destination)
                return;

            var executor = operation.Members.First(o => o.Type == Members.Executor);

            if (executor.Member is not IContainerOperator containerOperator
                || !containerOperator.HasAccessTo(this))
                throw new ContainerAccessException(executor, this);

            await base.UpstreamOperation(operation, context, data, member);
        }
    }

    public interface IContainer
    {
        int? OwnerId { get; }

        bool HasAccess(int id) => OwnerId == null || OwnerId.Equals(id);
    }

    public interface IContainerOperator
    {
        bool HasAccessTo(IContainer container);
    }

    public class Player : IContainerOperator
    {
        public int Id { get; }

        public Player(int id)
        {
            Id = id;
        }

        public bool HasAccessTo(IContainer container) => container.HasAccess(Id);
    }

    public class ContainerAccessException : Exception
    {
        public OperationMember Executor { get; }

        public ContainerAccessException(OperationMember executor, IContainer container)
            : base($"'{executor.Member}' has no access to '{container}'.")
        {
            Executor = executor;
        }
    }
}