using System;
using Weapons.User;

namespace Weapons.Tests.Mocks
{
    public class TestUser : IUser
    {
        private ButtonState _state;
        private int[]? _attachment;
        private bool _cancel;

        public void Update()
        {
            switch (_state)
            {
                case ButtonState.Down:
                    _state = ButtonState.Up;
                    break;
                case ButtonState.Up:
                    _state = ButtonState.Released;
                    break;
                case ButtonState.Pressed:
                case ButtonState.None:
                case ButtonState.Released:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (_cancel)
                _cancel = false;

            _attachment = null;
        }

        public void PressButton()
        {
            _state = ButtonState.Down;
        }
        
        public void PressCancel()
        {
            _cancel = true;
        }

        public void ToggleAttachments(params int[] attachment)
        {
            _attachment = attachment;
        }

        public ButtonState ReadButtonState() => _state;
        public int[]? ReadToggleAttachments() => _attachment;
        public bool ReadCancel() => _cancel;
    }
}