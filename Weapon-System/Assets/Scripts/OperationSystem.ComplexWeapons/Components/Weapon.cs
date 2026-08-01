using OperationSystem.Component;

namespace OperationSystem.ComplexWeapons.Components
{
    public interface IWeapon : IComponent
    {
    }

    public struct Weapon : IWeapon
    {
    }
}