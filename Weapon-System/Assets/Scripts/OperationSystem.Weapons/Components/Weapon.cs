using OperationSystem.Component;

namespace OperationSystem.Weapons.Components
{
    public interface IWeapon : IComponent
    {
    }

    public struct Weapon : IWeapon
    {
    }
}