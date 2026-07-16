using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Weapons.Operations;
using Weapons.User.Events;

namespace Weapons.Units.Attachments
{
    public interface IAttachmentController : IUnitController
    {
        IEnumerator PerformToggle(IAttachment unit, IOperation<IAttachment> operation);
        IEnumerator CancelToggle(IAttachment unit, IOperation<IAttachment> operation);
    }
    
    public class AttachmentController : IAttachmentController
    {
        public const string Start = "Control Start Toggle";
        public const string Perform = "Control Perform Toggle";
        public const string Cancel = "Control Cancel Toggle";
        
        public void Update(IUnit unit)
        {
        }

        public IEnumerator PerformToggle(IAttachment unit, IOperation<IAttachment> operation)
        {
            Debug.Log(Start);
            Debug.Log(unit.AttachmentNumber);
            
            var cancel = unit.User.EnumerateEvents().Any(e => e is CancelEvent);
            if (cancel)
                throw new Exception();

            yield return null;
            Debug.Log(Perform);
            
            cancel = unit.User.EnumerateEvents().Any(e => e is CancelEvent);
            if (cancel)
                throw new Exception();
        }

        public IEnumerator CancelToggle(IAttachment unit, IOperation<IAttachment> operation)
        {
            yield return null;
            Debug.Log(Cancel);
        }
    }
}