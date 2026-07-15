using System;
using Weapons.User;

namespace Weapons.Tests.Mocks
{
    public class TestUser : IUser
    {
        private ButtonState _state;
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
        }

        public void PressButton()
        {
            _state = ButtonState.Down;
        }
        
        public void PressCancel()
        {
            _cancel = true;
        }
        
        public ButtonState ReadButtonState()
        {
            return _state;
        }

        public bool ReadCancel()
        {
            return _cancel;
        }
    }
}