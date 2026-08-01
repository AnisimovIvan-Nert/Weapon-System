using System.Linq;
using Coroutine;
using NUnit.Framework;
using OperationSystem.ComplexWeapons.Assets;
using OperationSystem.ComplexWeapons.Operations;
using OperationSystem.ComplexWeapons.UnitHandlers;
using OperationSystem.Operations;
using OperationSystem.Operations.Middleware;
using OperationSystem.Units;

namespace OperationSystem.ComplexWeapons._Tests
{
    public class Tests
    {
        private const int Timeout = 100;
        
        [Test]
        public void ShotEmptyMagazineTest()
        {
            var magazine = new MagazineAsset(0);
            var weaponAsset = new WeaponAsset();
            weaponAsset.AddChild(magazine);

            var world = UnitWorld.Create();
            var operationRunner = new OperationRunner();

            var handler = new WeaponUnitHandler(operationRunner, world);
            handler.SetUnit(weaponAsset).Wait(Timeout);

            var identifier = OperationIdentifier.CreateNew();
            var operation = new WeaponShotOperation(identifier, Enumerable.Empty<IOperationMiddleware>());
            operation.RunOperation(handler);
            
            
        }
    }
}