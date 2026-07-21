using System.Collections.Generic;

namespace Weapons.Units.Implementations
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