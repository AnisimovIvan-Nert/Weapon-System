using System;

namespace Weapons.Reservable
{
    public interface IReservable<T> : IReservablePop<T>, IReservablePush<T>
    {
        
    }
    
    public interface IReservablePop<out T>
    {
        bool ReservePop(Guid guid);
        T Pop(Guid guid);
        void ReleasePop(Guid guid);
    }
    
    public interface IReservablePush<in T>
    {
        bool? ReservePush(Guid guid);
        void Push(Guid guid, T value);
        void ReleasePush(Guid guid);
    }
}