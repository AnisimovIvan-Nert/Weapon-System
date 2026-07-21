namespace Weapons
{
    public interface IInput
    {
        InputState Shoot { get; }
        InputState Cancel { get; }

        public enum InputState
        {
            None,
            Passive,
            Active,
            Activated,
            Deactivated,
        }
    }
}