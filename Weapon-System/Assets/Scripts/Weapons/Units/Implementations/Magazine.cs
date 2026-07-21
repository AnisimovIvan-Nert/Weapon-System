using System.Collections.Generic;

namespace Weapons.Units.Implementations
{
    public interface IMagazine : IUnit
    {
        int Rounds { get; set; }
    }

    public class Magazine
        : AbstractUnit
        , IMagazine
    {
        public int Rounds { get; set; }
        
        public Magazine(int rounds, IEnumerable<IUnit> children)
            : base(children)
        {
            Rounds = rounds;
        }
    }
}