namespace Weapons
{
    public interface IUnitData
    {
        string Name { get; }
    }

    public class UnitData : IUnitData
    {
        public string Name { get; }
        
        public UnitData(string name)
        {
            Name = name;
        }
    }
}