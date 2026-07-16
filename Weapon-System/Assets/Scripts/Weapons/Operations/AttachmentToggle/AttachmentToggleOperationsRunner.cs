using System.Collections.Generic;
using System.Linq;
using Weapons.Units.Attachments;
using Weapons.User.Events;

namespace Weapons.Operations.AttachmentToggle
{
    public class AttachmentToggleOperationsRunner : IOperationsRunner
    {
        private readonly List<AttachmentToggleOperation> _operations = new();

        public void Update(IUnit unit)
        {
            if (unit is not IAttachment attachment)
                return;
            
            HandleEvents(attachment);
            HandleOperations(attachment);
        }

        private void HandleOperations(IAttachment unit)
        {
            foreach (var operation in _operations)
                operation.Increment(unit);

            _operations.RemoveAll(operation => operation.State == OperationState.Destroying);
        }

        private void HandleEvents(IAttachment unit)
        {
            foreach (var userEvent in unit.User.EnumerateEvents())
            {
                switch (userEvent)
                {
                    case AttachmentToggleEvent toggleEvent:
                        if (toggleEvent.All || toggleEvent.Attachments.Contains(unit.AttachmentNumber))
                            _operations.Add(new AttachmentToggleOperation());
                        break;
                }
            }
        }
    }
}