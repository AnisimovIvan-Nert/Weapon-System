using OperationSystem.Component;

namespace OperationSystem.Weapons.Units
{
    public interface IWeapon : IComponent
    {
    }

    public class Weapon
        : AbstractComponent
        , IWeapon
    {
    }
}