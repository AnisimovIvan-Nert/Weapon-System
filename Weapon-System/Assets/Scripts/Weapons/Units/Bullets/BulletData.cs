namespace Weapons.Units.Bullets
{
    public interface IBulletData : IUnitData
    {
        
    }
    
    public class BulletData : IBulletData
    {
        public string Name { get; }
        
        public BulletData(string name)
        {
            Name = name;
        }
    }
}