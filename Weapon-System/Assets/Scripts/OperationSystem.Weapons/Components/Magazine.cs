using OperationSystem.Component;

namespace OperationSystem.Weapons.Components
{
    public interface IMagazine : IComponent
    {
        int Rounds { get; set; }
    }

    public struct Magazine : IMagazine
    {
        public int Rounds { get; set; }
        
        public Magazine(int rounds)
        {
            Rounds = rounds;
        }
    }
}