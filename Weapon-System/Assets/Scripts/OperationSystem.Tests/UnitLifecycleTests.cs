using NUnit.Framework;
using OperationSystem.Tests.Mocks;
using OperationSystem.Units;

namespace OperationSystem.Tests
{
    public class UnitLifecycleTests
    {
        [Test]
        public void CreateUnit()
        {
            var world = UnitWorld.Create();
            var foo = new FooAsset();

            var unit1 = world.GetOrCreateUnit(foo);
            var unit2 = world.GetOrCreateUnit(foo);
            
            Assert.AreEqual(unit1, unit2);
        }
    }
}