namespace Weapons
{
    public interface IWeaponData
    {
        string Name { get; }
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