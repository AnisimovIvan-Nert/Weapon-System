namespace Weapons.Operations
{
    public interface IOperationsRunner<in T>
        where T : IUnit
    {
        public void Update(T unit);
    }
}