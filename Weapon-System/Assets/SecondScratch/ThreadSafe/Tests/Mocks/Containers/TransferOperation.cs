using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SecondScratch.ThreadSafe.Operations;
using SecondScratch.ThreadSafe.Operations.Common;
using SecondScratch.ThreadSafe.Schedulers;

namespace SecondScratch.ThreadSafe.Tests.Mocks.Containers
{
    public interface ITransferContext : IOperationContext
    {
        public Player Player { get; set; }
        public Item Item { get; set; }

        ValueTask Validate();
    }
    
    public interface IAddContext : ITransferContext
    {
        public Container To { get; set; }

        ValueTask MutateTo();
        ValueTask RollbackTo();
    }

    public interface IRemoveContext : ITransferContext
    {
        public Container From { get; set; }

        ValueTask MutateFrom();
        ValueTask RollbackFrom();
    }
    
    public class TransferContext : IAddContext, IRemoveContext
    {
        public Player Player { get; set; }
        public Container From { get; set; }
        public Container To { get; set; }
        public Item Item { get; set; }
        
        public TransferContext(Player player, Container from, Container to, Item item)
        {
            Player = player;
            From = from;
            To = to;
            Item = item;
        }

        public ValueTask Validate()
        {
            if (!Player.HasAccessTo(From))
                throw new TransferAccessException(this, TransferAccessException.Member.From);
            if (!Player.HasAccessTo(To))
                throw new TransferAccessException(this, TransferAccessException.Member.To);

            return new ValueTask(Task.CompletedTask);
        }
        public async ValueTask MutateFrom() => await From.CreateRemoveChildCommand(Item).Schedule();
        public async ValueTask RollbackFrom() => await From.CreateAddChildCommand(Item).Schedule();
        
        public async ValueTask MutateTo() => await To.CreateAddChildCommand(Item).Schedule();
        public async ValueTask RollbackTo() => await To.CreateRemoveChildCommand(Item).Schedule();
    }

    public class TransferAccessException : Exception
    {
        public Member Source { get; }
        
        public TransferAccessException(TransferContext cxt, Member member)
            : base($"'{cxt.Player}' has no access to move '{cxt.Item}' between '{cxt.From}' -> '{cxt.To}'.")
        {
            Source = member;
        }

        public enum Member
        {
            None,
            From,
            To
        }
    }
    
    public class ContainerAddOperation : AbstractOperation<IAddContext>
    {
        private ContainerAddOperation(
            IEnumerable<IOperationSubject> subjects,
            IEnumerable<OperationTypes> types,
            IEnumerable<OperationMember> members) 
            : base(subjects, types, members)
        {
        }

        public static ContainerAddOperation Create(IAddContext context)
        {
            var subjects = new IOperationSubject[]
            {
                context.To
            };
            var types = new[]
            {
                OperationTypes.ContainerInteractionAdd,
            };
            var members = new OperationMember[]
            {
                new(Operations.Common.Members.Executor, context.Player),
                new(Operations.Common.Members.Subject, context.Item),
                new(Operations.Common.Members.Destination, context.To)
            };
            return new ContainerAddOperation(subjects, types, members);
        }

        protected override async ValueTask InnerExecute(IAddContext context, OperationData data)
        {
            var transaction = data.Transaction;

            await context.Validate();
            await transaction.Apply(context.MutateTo, context.RollbackTo);
        }
    }

    public class ContainerRemoveOperation : AbstractOperation<IRemoveContext>
    {
        private ContainerRemoveOperation(
            IEnumerable<IOperationSubject> subjects,
            IEnumerable<OperationTypes> types,
            IEnumerable<OperationMember> members) 
            : base(subjects, types, members)
        {
        }

        public static ContainerRemoveOperation Create(IRemoveContext context)
        {
            var subjects = new IOperationSubject[]
            {
                context.From
            };
            var types = new[]
            {
                OperationTypes.ContainerInteractionRemove
            };
            var members = new OperationMember[]
            {
                new(Operations.Common.Members.Executor, context.Player),
                new(Operations.Common.Members.Subject, context.Item),
                new(Operations.Common.Members.Source, context.From)
            };
            return new ContainerRemoveOperation(subjects, types, members);
        }

        protected override async ValueTask InnerExecute(IRemoveContext context, OperationData data)
        {
            var transaction = data.Transaction;

            await context.Validate();
            await transaction.Apply(context.MutateFrom, context.RollbackFrom);
        }
    }
}