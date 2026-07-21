using System;
using System.Collections.Generic;
using System.Linq;

namespace Weapons.Reservable
{
    public class ReservableCollectionHelper<T, TValue> : IReservable<TValue>
        where TValue : T
    {
        private readonly ICollection<T> _collection;
        private readonly int? _size;

        private Guid? _reservedBy;
        
        public ReservableCollectionHelper( ICollection<T> collection, int? size = null)
        {
            _collection = collection;
            _size = size;
        }

        public bool ReservePop(Guid guid)
        {
            if (_reservedBy != null)
                return false;

            if (_collection.OfType<TValue>().Any())
                return false;

            _reservedBy = guid;
            return true;
        }

        public TValue Pop(Guid guid)
        {
            if (_reservedBy != guid)
                throw new InvalidOperationException();

            _reservedBy = null;
            return _collection.OfType<TValue>().First();
        }

        public void ReleasePop(Guid guid)
        {
            if (_reservedBy != guid)
                throw new InvalidOperationException();

            _reservedBy = null;
        }

        public bool? ReservePush(Guid guid)
        {
            if (_reservedBy != null)
                return false;

            if (_size != null && _collection.OfType<TValue>().Count() >= _size)
                return false;

            _reservedBy = guid;
            return true;
        }

        public void Push(Guid guid, TValue value)
        {
            if (_reservedBy != guid)
                throw new InvalidOperationException();
            
            _reservedBy = null;
            
            if (_size != null && _collection.OfType<TValue>().Count() >= _size)
                throw new InvalidOperationException();
            
            _collection.Add(value);
        }

        public void ReleasePush(Guid guid)
        {
            if (_reservedBy != guid)
                throw new InvalidOperationException();

            _reservedBy = null;
        }
    }
}