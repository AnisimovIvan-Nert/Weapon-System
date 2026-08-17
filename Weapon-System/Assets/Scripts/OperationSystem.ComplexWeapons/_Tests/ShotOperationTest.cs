using System;
using NUnit.Framework;
using OperationSystem.Assets;
using OperationSystem.ComplexWeapons._Tests.Mocks;
using OperationSystem.ComplexWeapons.Assets;
using OperationSystem.ComplexWeapons.Operations.Hit.Handle;
using OperationSystem.ComplexWeapons.Operations.Hit.Result;
using OperationSystem.ComplexWeapons.Operations.Shot;
using OperationSystem.ComplexWeapons.Operations.Shot.Stages;
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

        private RaycastHitCollectorMiddleware _raycastHitCollectorMiddleware = null!;

        [SetUp]
        public void SetUp()
        {
            _raycastHitCollectorMiddleware = new RaycastHitCollectorMiddleware();
        }
        
        [Test]
        public void EmptyMagazineFailTest()
        {
            const int rounds = 0;
            
            var gameObject = new GameObject();
            var world = CreateWorld();
            
            var weaponAsset = CreateWeapon(gameObject, rounds, out var magazine);

            var operation = RunAndWaitOperation(weaponAsset, world);
            AssertFail<TakeBulletFromMagazineOperation.MagazineIsEmptyException>(operation, magazine, rounds);
            
            Assert.AreEqual(0, _raycastHitCollectorMiddleware.Hits.Count);
        }

        [Test]
        public void NotHitPassTest()
        {
            const int rounds = 1;
            
            var gameObject = new GameObject();
            var world = CreateWorld();
            
            var weaponAsset = CreateWeapon(gameObject, rounds, out var magazine);
            
            var operation = RunAndWaitOperation(weaponAsset, world);
            AssertPass(operation, magazine, rounds);

            if (operation.OperationResult is not WeaponShotOperation.Result result)
                throw new InvalidOperationException();
            
            world.UpdateUntilComplete(result.Operation);
            result.Operation.AssertPass();
            if (result.Operation.OperationResult is not NotHitResult)
                throw new InvalidOperationException();
            
            Assert.AreEqual(1, _raycastHitCollectorMiddleware.Hits.Count);
        }
        
        [Test]
        public void PlayerHitPassTest()
        {
            const int rounds = 1;
            
            var gameObject = new GameObject();
            var world = CreateWorld();
            
            var weaponAsset = CreateWeapon(gameObject, rounds, out var magazine);
            
            var hit = CreateFakeHit<PlayerAsset>();
            var fakeMiddleware = new FakeRaycastMiddleware(hit);
            
            var operation = RunAndWaitOperation(weaponAsset, world, fakeMiddleware);
            AssertPass(operation, magazine, rounds);

            if (operation.OperationResult is not WeaponShotOperation.Result result)
                throw new InvalidOperationException();
            
            world.UpdateUntilComplete(result.Operation);
            result.Operation.AssertPass();
            if (result.Operation.OperationResult is not PlayerHitHandleOperation.Result)
                throw new InvalidOperationException();
            
            Assert.AreEqual(1, _raycastHitCollectorMiddleware.Hits.Count);
        }
        
        [Test]
        public void ObstacleHitPassTest()
        {
            const int rounds = 1;
            
            var gameObject = new GameObject();
            var world = CreateWorld();
            
            var weaponAsset = CreateWeapon(gameObject, rounds, out var magazine);
            
            var hit = CreateFakeHit<ObstacleAsset>();
            var fakeMiddleware = new FakeRaycastMiddleware(hit, 2);
            
            var operation = RunAndWaitOperation(weaponAsset, world, fakeMiddleware);
            AssertPass(operation, magazine, rounds);

            if (operation.OperationResult is not WeaponShotOperation.Result result)
                throw new InvalidOperationException();
            
            world.UpdateUntilComplete(result.Operation);
            result.Operation.AssertPass();
            if (result.Operation.OperationResult is not NotHitResult)
                throw new InvalidOperationException();
            
            Assert.AreEqual(3, _raycastHitCollectorMiddleware.Hits.Count);
        }

        private UnitWorld CreateWorld()
        {
            var middlewares = new IOperationMiddleware[]
            {
                _raycastHitCollectorMiddleware
            };
            
            var world = UnitWorld.Create();
            world.AppendMiddlewares(middlewares);
            return world;
        }
        
        private static WeaponAsset CreateWeapon(GameObject gameObject, int rounds, out MagazineAsset magazine)
        {
            magazine = gameObject.AddComponent<MagazineAsset>();
            magazine.Rounds = rounds;
            var barrel = gameObject.AddComponent<BarrelAsset>();
            var weaponAsset = gameObject.AddComponent<WeaponAsset>();
            weaponAsset.TryAddChild(magazine);
            weaponAsset.TryAddChild(barrel);
            return weaponAsset;
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
            Assert.IsNotNull(operation.OperationResult);
        }

        private static WeaponShotOperation RunAndWaitOperation(WeaponAsset weaponAsset, UnitWorld world, params IOperationMiddleware[] middlewares)
        {
            var unit = world.GetOrCreateUnit(weaponAsset);

            var identifier = OperationIdentifier.CreateNew();
            var data = new WeaponShotOperation.Data(10);
            var operationUnit = new OperationUnit(unit);
            var operation = new WeaponShotOperation(identifier, data, operationUnit);
            operation.RunOperationOnWorld(world, middlewares: middlewares);

            world.UpdateUntilComplete(operation, Timeout);
            return operation;
        }

        private static RaycastHit CreateFakeHit<T>()
            where T : MonoBehaviour, IAsset
        {
            var gameObject = new GameObject();
            gameObject.AddComponent<T>();
            gameObject.AddComponent<BoxCollider>();

            var direction = Vector3.forward;
            var origin = gameObject.transform.position - direction;
            Physics.Raycast(origin, direction, out var hit, 100, ~0);
            return hit;
        }
    }
}