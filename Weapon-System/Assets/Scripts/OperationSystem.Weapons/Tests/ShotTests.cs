using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using NUnit.Framework;
using OperationSystem.Operations;
using OperationSystem.Operations.Data;
using OperationSystem.TestExtensions;
using OperationSystem.Weapons.Assets;
using OperationSystem.Weapons.Operations;
using UnityEngine;

namespace OperationSystem.Weapons.Tests
{
    public class ShotTests
    {
        private const int Timeout = 100;

        private const int Rounds = 10;

        [Test]
        public void MultipleSimultaneouslyShotOperations()
        {
            var gameObject = new GameObject();
            var pistol = gameObject.AddComponent<Pistol>();
            var chamber = gameObject.AddComponent<PistolChamber>();
            chamber.hasRound = false;
            var magazine = gameObject.AddComponent<PistolMagazine>();
            magazine.rounds = Rounds;
            
            pistol.TryAddChild(chamber);
            pistol.TryAddChild(magazine);

            var operationRunner = new OperationRunner();
            
            var operations = new List<IOperation>();
            for (var i = 0; i < Rounds + 1; i++)
            {
                var identifier = OperationIdentifier.CreateNew();
                var operationAsset = new OperationAsset(pistol);
                var operation = new WeaponShotUnitOperation(identifier, operationAsset);
                operation.RunOperation(operationRunner);
                operations.Add(operation);
            }
            
            operationRunner.UpdateUntilComplete(operations.ToArray(), Timeout);

            var failedOperation = operations.SingleOrDefault(o => !o.IsCompletedSuccessfully);
            Assert.NotNull(failedOperation);
            
            Assert.False(chamber.hasRound);
            Assert.Zero(magazine.rounds);
        }
        
        [Test]
        public void LoadedChamberPassTest()
        {
            var gameObject = new GameObject();
            var pistol = gameObject.AddComponent<Pistol>();
            var chamber = gameObject.AddComponent<PistolChamber>();
            chamber.hasRound = true;
            
            pistol.TryAddChild(chamber);
            var operation = RunAndWaitOperation(pistol);
            AssertPass(operation, 0, true, chamber, null);
        }
        
        [Test]
        public void EmptyChamberPassTest()
        {
            var gameObject = new GameObject();
            var pistol = gameObject.AddComponent<Pistol>();
            var chamber = gameObject.AddComponent<PistolChamber>();
            chamber.hasRound = false;
            var magazine = gameObject.AddComponent<PistolMagazine>();
            magazine.rounds = Rounds;
            
            pistol.TryAddChild(chamber);
            pistol.TryAddChild(magazine);
            var operation = RunAndWaitOperation(pistol);
            AssertPass(operation, Rounds, false, chamber, magazine);
        }
        
        [Test]
        public void EmptyMagazineFailTest()
        {
            var gameObject = new GameObject();
            var pistol = gameObject.AddComponent<Pistol>();
            var chamber = gameObject.AddComponent<PistolChamber>();
            chamber.hasRound = false;
            var magazine = gameObject.AddComponent<PistolMagazine>();
            magazine.rounds = 0;
            
            pistol.TryAddChild(chamber);
            pistol.TryAddChild(magazine);
            var operation = RunAndWaitOperation(pistol);
            AssertFail(operation, 0, false, chamber, magazine);
        }
        
        [Test]
        public void NoMagazineFailTest()
        {
            var gameObject = new GameObject();
            var pistol = gameObject.AddComponent<Pistol>();
            var chamber = gameObject.AddComponent<PistolChamber>();
            chamber.hasRound = false;
            
            pistol.TryAddChild(chamber);
            var operation = RunAndWaitOperation(pistol);
            AssertFail(operation, 0, false, chamber, null);
        }
        
        [Test]
        public void NoChamberFailTest()
        {
            var gameObject = new GameObject();
            var pistol = gameObject.AddComponent<Pistol>();
            var magazine = gameObject.AddComponent<PistolMagazine>();
            magazine.rounds = Rounds;
            pistol.TryAddChild(magazine);
            var operation = RunAndWaitOperation(pistol);
            AssertFail(operation, Rounds, false, null, magazine);
        }

        private static IOperation RunAndWaitOperation(Pistol pistol)
        {
            var operationRunner = new OperationRunner();

            var identifier = OperationIdentifier.CreateNew();
            var operationAsset = new OperationAsset(pistol);
            var operation = new WeaponShotUnitOperation(identifier, operationAsset);
            operation.RunOperation(operationRunner);
            operationRunner.UpdateUntilComplete(operation, Timeout);
            
            return operation;
        }
        
        private static void AssertPass(IOperation operation, int rounds, bool hasRound, PistolChamber? chamber, PistolMagazine? magazine)
        {
            if (operation.Exception != null)
                ExceptionDispatchInfo.Capture(operation.Exception).Throw();
            
            operation.AssertPass();
            
            if (chamber != null)
                Assert.AreEqual(false, chamber.hasRound);
            
            var exceptedRounds = hasRound ? rounds : rounds - 1;
            if (magazine != null)
                Assert.AreEqual(exceptedRounds, magazine.rounds);
        }

        private static void AssertFail(IOperation operation, int rounds, bool hasRound, PistolChamber? chamber, PistolMagazine? magazine)
        {
            operation.AssertFail();
            
            if (chamber != null)
                Assert.AreEqual(hasRound, chamber.hasRound);
            
            if (magazine != null)
                Assert.AreEqual(rounds, magazine.rounds);
        }
    }
}