using System;
using System.Collections.Generic;
using Weapons.User.Events;

namespace Weapons.User
{
    public class UserAdapter : IUserAdapter
    {
        private readonly IUser _user;

        private readonly List<IUserEvent> _events = new();

        public UserAdapter(IUser user)
        {
            _user = user;
        }

        public void Update()
        {
            _events.Clear();
            
            switch (_user.ReadButtonState())
            {
                case ButtonState.Down:
                    _events.Add(new ShootStartEvent());
                    break;
                case ButtonState.Up:
                    _events.Add(new ShootEndEvent());
                    break;
                case ButtonState.None:
                case ButtonState.Pressed:
                case ButtonState.Released:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (_user.ReadCancel())
                _events.Add(new CancelEvent());

            var toggleAttachments = _user.ReadToggleAttachments();
            if (toggleAttachments != null)
                _events.Add(new AttachmentToggleEvent(toggleAttachments));
        }

        public IEnumerable<IUserEvent> EnumerateEvents() => _events;
    }
}