using System.Linq;
using Weapons.Units.Attachments;
using Weapons.User.Events;

namespace Weapons.Operations.AttachmentToggle
{
    public class AttachmentToggleOperationsRunner : AbstractOperationsRunner<IAttachment>
    {
        protected override void HandleEvents(IAttachment unit)
        {
            foreach (var userEvent in unit.User.EnumerateEvents())
            {
                switch (userEvent)
                {
                    case AttachmentToggleEvent toggleEvent:
                        if (toggleEvent.All || toggleEvent.Attachments.Contains(unit.AttachmentNumber))
                            Operations.Add(new AttachmentToggleOperation());
                        break;
                }
            }
        }
    }
}