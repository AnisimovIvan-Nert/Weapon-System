namespace Weapons
{
    public class UnitData : IUnitData
    {
        public string Name { get; }
        
        public UnitData(string name)
        {
            Name = name;
        }
    }
}