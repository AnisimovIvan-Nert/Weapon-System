namespace Weapons.Units.Magazines
{
    public interface IMagazineData : IUnitData
    {
        
    }
    
    public class MagazineData : IMagazineData
    {
        public string Name { get; }
        
        public MagazineData(string name)
        {
            Name = name;
        }
    }
}