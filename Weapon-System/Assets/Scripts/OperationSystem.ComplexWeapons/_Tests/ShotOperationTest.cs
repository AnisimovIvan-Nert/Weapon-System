using System;
using NUnit.Framework;
using OperationSystem.ComplexWeapons.Assets;
using OperationSystem.ComplexWeapons.Components;
using OperationSystem.ComplexWeapons.Operations;
using OperationSystem.Operations;
using OperationSystem.Operations.Data;
using OperationSystem.Operations.Middleware;
using OperationSystem.Operations.Result;
using OperationSystem.TestExtensions;
using OperationSystem.Units;
using UnityEngine;

namespace OperationSystem.ComplexWeapons._Tests
{
    public class ShotOperationTest
    {
        private const int Timeout = 100;
        
        [Test]
        public void EmptyMagazineFailTest()
        {
            const int rounds = 0;
            
            var gameObject = new GameObject();
            var magazine = gameObject.AddComponent<MagazineAsset>();
            magazine.Rounds = rounds;
            var barrel = gameObject.AddComponent<BarrelAsset>();
            var weaponAsset = gameObject.AddComponent<WeaponAsset>();
            weaponAsset.AddChild(magazine);
            weaponAsset.AddChild(barrel);

            var operation = RunAndWaitOperation(weaponAsset);
            AssertFail<WeaponShotOperation.MagazineIsEmptyException>(operation, magazine, rounds);
        }

        [Test]
        public void PassTest()
        {
            const int rounds = 1;
            
            var gameObject = new GameObject();
            var magazine = gameObject.AddComponent<MagazineAsset>();
            magazine.Rounds = rounds;
            var barrel = gameObject.AddComponent<BarrelAsset>();
            var weaponAsset = gameObject.AddComponent<WeaponAsset>();
            weaponAsset.AddChild(magazine);
            weaponAsset.AddChild(barrel);
            
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
            var unit = world.GetOrCreateUnit(weaponAsset);

            var identifier = OperationIdentifier.CreateNew();
            var data = new WeaponShotOperation.Data(10);
            var operationUnit = new OperationUnit(unit);
            var operation = new WeaponShotOperation(identifier, data, operationUnit, Array.Empty<IOperationMiddleware>());
            operation.RunOperationOnWorld(world);

            world.UpdateUntilComplete(operation, Timeout);
            return operation;
        }
    }
}