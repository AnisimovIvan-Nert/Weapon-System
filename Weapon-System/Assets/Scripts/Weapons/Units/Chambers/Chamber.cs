using System;
using System.Collections.Generic;
using System.Linq;
using Weapons.Operations;
using Weapons.ProducerConsumer;
using Weapons.Reservable;
using Weapons.Units.Bullets;

namespace Weapons.Units.Chambers
{
    public interface IChamber : IUnit<IChamberData>, IReservable<IBullet>
    {
    }

    public class Chamber
        : AbstractUnit<IChamberData>
        , IChamber
    {
        private readonly IEnumerable<IUnit> _children;
        private readonly IList<IBullet> _bullets;
        private readonly ReservableCollectionHelper<IBullet, IBullet> _reservableHelper;

        public override IEnumerable<IUnit> Children => _children.Concat(_bullets);

        public Chamber(
            IEventProducer eventProducer,
            IOperationRunner runner,
            IChamberData data,
            params IUnit[] children)
            : base(eventProducer, runner, children, data)
        {
            _children = children.Where(o => o is not IBullet).ToList();
            _bullets = children.OfType<IBullet>().ToList();
            _reservableHelper = new ReservableCollectionHelper<IBullet, IBullet>(_bullets, 1);
        }
        
        public bool ReservePop(Guid guid) => _reservableHelper.ReservePop(guid);
        public IBullet Pop(Guid guid) => _reservableHelper.Pop(guid);
        public void ReleasePop(Guid guid) => _reservableHelper.ReleasePop(guid);
        public bool? ReservePush(Guid guid) => _reservableHelper.ReservePush(guid);
        public void Push(Guid guid, IBullet value) => _reservableHelper.Push(guid, value);
        public void ReleasePush(Guid guid) => _reservableHelper.ReleasePush(guid);
    }
}