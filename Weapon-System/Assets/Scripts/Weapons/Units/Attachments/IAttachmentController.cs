using System.Threading.Tasks;
using Weapons.Operations;

namespace Weapons.Units.Attachments
{
    public interface IAttachmentController : IUnitController
    {
        Task PerformToggle(IAttachment unit, IOperation<IAttachment> operation);
        Task CancelToggle(IAttachment unit, IOperation<IAttachment> operation);
    }
}