using System.Collections.Generic;
using OperationSystem.Units;

namespace OperationSystem.Weapons.Units
{
    public interface IChamber : IUnit
    {
        bool HasRound { get; set; }
    }

    public class Chamber
        : AbstractUnit
        , IChamber
    {
        public bool HasRound { get; set; }
        
        public Chamber(bool hasRound, IEnumerable<IUnit> children)
            : base(children)
        {
            HasRound = hasRound;
        }
    }
}