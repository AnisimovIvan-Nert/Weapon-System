using System.Linq;
using Coroutine;
using NUnit.Framework;
using OperationSystem.ComplexWeapons.Assets;
using OperationSystem.ComplexWeapons.Operations;
using OperationSystem.ComplexWeapons.UnitHandlers;
using OperationSystem.Operations;
using OperationSystem.Operations.Middleware;
using OperationSystem.Operations.Result;
using OperationSystem.Tests;
using OperationSystem.Units;

namespace OperationSystem.ComplexWeapons._Tests
{
    public class ShotOperationTest
    {
        private const int Timeout = 100;
        
        [Test]
        public void EmptyMagazineFailTest()
        {
            const int rounds = 0;
            
            var magazine = new MagazineAsset(rounds);
            var weaponAsset = new WeaponAsset();
            weaponAsset.AddChild(magazine);

            var operation = RunAndWaitOperation(weaponAsset);
            AssertFail<WeaponShotOperation.MagazineIsEmptyException>(operation, magazine, rounds);
        }

        [Test]
        public void PassTest()
        {
            const int rounds = 1;
            
            var magazine = new MagazineAsset(rounds);
            var weaponAsset = new WeaponAsset();
            weaponAsset.AddChild(magazine);
            
            var operation = RunAndWaitOperation(weaponAsset);
            AssertPass(operation, magazine, rounds);
        }
        
        private static void AssertFail<T>(WeaponShotOperation operation, MagazineAsset magazine, int rounds)
            where T : OperationException
        {
            operation.AssertFail<T>();
            Assert.AreEqual(rounds, magazine.Rounds);
        }
        
        private static void AssertPass(WeaponShotOperation operation, MagazineAsset magazine, int rounds)
        {
            operation.AssertPass();
            Assert.AreEqual(rounds - 1, magazine.Rounds);
        }

        private static WeaponShotOperation RunAndWaitOperation(WeaponAsset weaponAsset)
        {
            var world = UnitWorld.Create();
            var operationRunner = new OperationRunner();

            var handler = new WeaponUnitHandler(operationRunner, world);
            handler.SetUnit(weaponAsset).Wait(Timeout);

            var identifier = OperationIdentifier.CreateNew();
            var operation = new WeaponShotOperation(identifier, Enumerable.Empty<IOperationMiddleware>());
            operation.RunOperation(handler);

            handler.UpdateUntilComplete(operation, Timeout);
            return operation;
        }
    }
}