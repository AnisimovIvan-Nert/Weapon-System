namespace SecondScratch.ThreadSafe.Tests.Mocks
{
    public partial class Player
    {
        public int Health { get; private set; }

        public void IncreaseHealth(int value)
        {
            Health += value;
        }

        public void Reset()
        {
            Health = 0;
        }
    }
}