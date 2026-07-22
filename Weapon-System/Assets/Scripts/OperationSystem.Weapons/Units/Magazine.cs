using System.Collections.Generic;
using OperationSystem.Units;

namespace OperationSystem.Weapons.Units
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