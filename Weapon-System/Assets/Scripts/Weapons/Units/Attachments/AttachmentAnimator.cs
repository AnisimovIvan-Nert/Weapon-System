using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Weapons.Operations;
using Weapons.User.Events;

namespace Weapons.Units.Attachments
{
    public interface IAttachmentAnimator : IUnitAnimator
    {
        IEnumerator PerformToggle(IAttachment unit, IOperation<IAttachment> operation);
        IEnumerator CancelToggle(IAttachment unit, IOperation<IAttachment> operation);
    }
    
    public class AttachmentAnimator : IAttachmentAnimator
    {
        public const string Start = "Animate Start Toggle";
        public const string Perform = "Animate Perform Toggle";
        public const string Cancel = "Animate Cancel Toggle";
        
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