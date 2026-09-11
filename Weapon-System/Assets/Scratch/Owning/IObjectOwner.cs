using System;
using System.Threading.Tasks;

namespace Scratch.Owning
{
    public interface IObjectOwner
    {
        ValueTask RunOnOwner(Action action);
        ValueTask<T> RunOnOwner<T>(Func<T> func);

        bool TryRunImmediately(Action action);
        bool TryRunImmediately<T>(Func<T> func, out T result);

        void Enqueue(Action action);
    }
}