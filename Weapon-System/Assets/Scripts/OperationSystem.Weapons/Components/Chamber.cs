using OperationSystem.Component;

namespace OperationSystem.Weapons.Components
{
    public interface IChamber : IComponent
    {
        bool HasRound { get; set; }
    }

    public struct Chamber : IChamber
    {
        public bool HasRound { get; set; }
        
        public Chamber(bool hasRound)
        {
            HasRound = hasRound;
        }
    }
}