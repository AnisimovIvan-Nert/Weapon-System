namespace Weapons.Units.Chambers
{
    public interface IChamberData : IUnitData
    {
        
    }
    
    public class ChamberData : IChamberData
    {
        public string Name { get; }
        
        public ChamberData(string name)
        {
            Name = name;
        }
    }
}