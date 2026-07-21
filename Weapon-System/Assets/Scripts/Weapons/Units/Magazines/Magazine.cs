using System;
using System.Collections.Generic;
using System.Linq;
using Weapons.Operations;
using Weapons.ProducerConsumer;
using Weapons.Reservable;
using Weapons.Units.Bullets;

namespace Weapons.Units.Magazines
{
    public interface IMagazine : IUnit<IMagazineData>, IReservable<IBullet>
    {
    }

    public class Magazine
        : AbstractUnit<IMagazineData>
        , IMagazine
    {
        private readonly IList<IUnit> _children;
        private readonly ReservableCollectionHelper<IUnit, IBullet> _reservableHelper;
        
        public override IEnumerable<IUnit> Children => _children;

        public Magazine(
            IEventProducer eventProducer,
            IOperationRunner runner,
            IMagazineData data,
            params IUnit[] children)
            : base(eventProducer, runner, children, data)
        {
            _children = children.ToList();
            _reservableHelper = new ReservableCollectionHelper<IUnit, IBullet>(_children);
        }

        public bool ReservePop(Guid guid) => _reservableHelper.ReservePop(guid);
        public IBullet Pop(Guid guid) => _reservableHelper.Pop(guid);
        public void ReleasePop(Guid guid) => _reservableHelper.ReleasePop(guid);
        public bool? ReservePush(Guid guid) => _reservableHelper.ReservePush(guid);
        public void Push(Guid guid, IBullet value) => _reservableHelper.Push(guid, value);
        public void ReleasePush(Guid guid) => _reservableHelper.ReleasePush(guid);
    }
}