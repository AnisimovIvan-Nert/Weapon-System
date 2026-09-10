using System;
using System.Threading;
using System.Threading.Tasks;

namespace Scratch.Owning
{
    public class ObjectOwnerHandle : IObjectOwnerHandle
    {
        private IObjectOwner _owner;
        private readonly ReaderWriterLockSlim _ownerLock = new();

        public ObjectOwnerHandle(IObjectOwner owner)
        {
            _owner = owner;
        }

        public ValueTask RunOnOwner(Action action)
        {
            _ownerLock.EnterReadLock();
            try
            {
                return _owner.RunOnOwner(action);
            }
            finally
            {
                _ownerLock.ExitReadLock();
            }
        }

        public ValueTask<T> RunOnOwner<T>(Func<T> func)
        {
            _ownerLock.EnterReadLock();
            try
            {
                return _owner.RunOnOwner(func);
            }
            finally
            {
                _ownerLock.ExitReadLock();
            }
        }

        public ValueTask Terminate()
        {
            return _owner.Terminate();
        }

        public ValueTask ChangeOwner(IObjectOwner owner)
        {
            _ownerLock.EnterWriteLock();
            try
            {
                var task = _owner.Terminate();
                _owner = owner;
                return task;
            }
            finally
            {
                _ownerLock.ExitWriteLock();
            }
        }
    }
}