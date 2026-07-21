using System;

namespace Weapons.Tests.Mocks
{
    public class TestInput : IInput
    {
        public IInput.InputState Shoot { get; private set; }
        public IInput.InputState Cancel { get; private set; }

        public void Update()
        {
            switch (Shoot)
            {
                case IInput.InputState.Active:
                case IInput.InputState.Activated:
                    Shoot = IInput.InputState.Deactivated;
                    break;
                case IInput.InputState.Passive:
                case IInput.InputState.None:
                case IInput.InputState.Deactivated:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (Cancel)
            {
                case IInput.InputState.Active:
                case IInput.InputState.Activated:
                    Cancel = IInput.InputState.Deactivated;
                    break;
                case IInput.InputState.Passive:
                case IInput.InputState.None:
                case IInput.InputState.Deactivated:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void PressShoot()
        {
            Shoot = IInput.InputState.Activated;
        }

        public void PressCancel()
        {
            Cancel = IInput.InputState.Activated;
        }
    }
}