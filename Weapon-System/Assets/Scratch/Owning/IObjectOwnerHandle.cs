using System.Threading.Tasks;

namespace Scratch.Owning
{
    public interface IObjectOwnerHandle : IObjectOwner
    {
        ValueTask ChangeOwner(IObjectOwner owner);
    }
}