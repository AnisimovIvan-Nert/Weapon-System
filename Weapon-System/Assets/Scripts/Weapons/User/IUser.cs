namespace Weapons.User
{
    public interface IUser
    {
        public ButtonState ReadButtonState();

        public int[]? ReadToggleAttachments();
        
        public bool ReadCancel();
    }

    public enum ButtonState
    {
        None,
        Down,
        Pressed,
        Up,
        Released
    }
}