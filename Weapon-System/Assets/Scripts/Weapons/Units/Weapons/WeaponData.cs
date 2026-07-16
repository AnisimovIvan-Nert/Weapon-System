namespace Weapons.Units.Weapons
{
    public interface IWeaponData : IUnitData
    {
        
    }
    
    public class WeaponData : IWeaponData
    {
        public string Name { get; }
        
        public WeaponData(string name)
        {
            Name = name;
        }
    }
}