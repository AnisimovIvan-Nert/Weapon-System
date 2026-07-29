using OperationSystem.Component;

namespace OperationSystem.Weapons.Components
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