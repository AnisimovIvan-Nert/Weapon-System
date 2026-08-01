using OperationSystem.Component;

namespace OperationSystem.ComplexWeapons.Components
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