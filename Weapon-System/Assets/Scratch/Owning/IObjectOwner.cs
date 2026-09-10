using System;
using System.Threading.Tasks;

namespace Scratch.Owning
{
    public interface IObjectOwner
    {
        ValueTask RunOnOwner(Action action);
        ValueTask<T> RunOnOwner<T>(Func<T> func);

        ValueTask Terminate();
    }
}