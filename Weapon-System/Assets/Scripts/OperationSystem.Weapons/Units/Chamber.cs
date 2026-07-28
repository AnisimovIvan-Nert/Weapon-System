using OperationSystem.Component;

namespace OperationSystem.Weapons.Units
{
    public interface IChamber : IComponent
    {
        bool HasRound { get; set; }
    }

    public class Chamber
        : AbstractComponent
        , IChamber
    {
        public bool HasRound { get; set; }
        
        public Chamber(bool hasRound)
        {
            HasRound = hasRound;
        }
    }
}