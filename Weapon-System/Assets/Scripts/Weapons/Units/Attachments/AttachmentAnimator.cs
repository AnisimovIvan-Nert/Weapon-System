using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Weapons.Operations;
using Weapons.User.Events;

namespace Weapons.Units.Attachments
{
    public class AttachmentAnimator : IAttachmentAnimator
    {
        public const string Start = "Animate Start Toggle";
        public const string Perform = "Animate Perform Toggle";
        public const string Cancel = "Animate Cancel Toggle";
        
        public void Update(IUnit unit)
        {
        }

        public async Task PerformToggle(IAttachment unit, IOperation<IAttachment> operation)
        {
            Debug.Log(Start);
            Debug.Log(unit.AttachmentNumber);
            
            var cancel = unit.User.EnumerateEvents().Any(e => e is CancelEvent);
            if (cancel)
                throw new Exception();
            
            await Task.Yield();
            Debug.Log(Perform);
            
            cancel = unit.User.EnumerateEvents().Any(e => e is CancelEvent);
            if (cancel)
                throw new Exception();
        }

        public async Task CancelToggle(IAttachment unit, IOperation<IAttachment> operation)
        {
            await Task.Yield();
            Debug.Log(Cancel);
        }
    }
}