using OperationSystem.Component;

namespace OperationSystem.Weapons.Units
{
    public interface IMagazine : IComponent
    {
        int Rounds { get; set; }
    }

    public class Magazine
        : AbstractComponent
        , IMagazine
    {
        public int Rounds { get; set; }
        
        public Magazine(int rounds)
        {
            Rounds = rounds;
        }
    }
}