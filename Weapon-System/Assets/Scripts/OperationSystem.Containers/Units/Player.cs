using OperationSystem.Units;

namespace OperationSystem.Containers.Units
{
    public interface IPlayer : IUnit
    {
    }

    public class Player
        : AbstractUnit
        , IPlayer
    {
        public Player(params IUnit[] children) 
            : base(children)
        {
        }
    }
}