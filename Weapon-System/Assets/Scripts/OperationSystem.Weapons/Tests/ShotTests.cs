using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using NUnit.Framework;
using OperationSystem.Operations;
using OperationSystem.Operations.Data;
using OperationSystem.Operations.Middleware;
using OperationSystem.TestExtensions;
using OperationSystem.Units;
using OperationSystem.Weapons.Assets;
using OperationSystem.Weapons.Operations;

namespace OperationSystem.Weapons.Tests
{
    public class ShotTests
    {
        private const int Timeout = 100;

        private const int Rounds = 10;

        [Test]
        public void MultipleSimultaneouslyShotOperations()
        {
            var pistol = new Pistol();
            var chamber = new PistolChamber(false);
            var magazine = new PistolMagazine(Rounds);
            pistol.AddChild(chamber);
            pistol.AddChild(magazine);

            var world = UnitWorld.Create();
            var unit = world.GetOrCreateUnit(pistol);
            
            var operations = new List<IOperation>();
            for (var i = 0; i < Rounds + 1; i++)
            {
                var identifier = OperationIdentifier.CreateNew();
                var operationUnit = new OperationUnit(unit);
                var operation = new WeaponShotUnitOperation(identifier, operationUnit, Array.Empty<IOperationMiddleware>());
                operation.RunOperation(world);
                operations.Add(operation);
            }

            world.UpdateUntilComplete(operations.ToArray(), Timeout);

            var failedOperation = operations.SingleOrDefault(o => !o.IsCompletedSuccessfully);
            Assert.NotNull(failedOperation);
            
            Assert.False(chamber.HasRound);
            Assert.Zero(magazine.Rounds);
        }
        
        [Test]
        public void LoadedChamberPassTest()
        {
            var pistol = new Pistol();
            var chamber = new PistolChamber(true);
            pistol.AddChild(chamber);
            var operation = RunAndWaitOperation(pistol);
            AssertPass(operation, 0, true, chamber, null);
        }
        
        [Test]
        public void EmptyChamberPassTest()
        {
            var pistol = new Pistol();
            var chamber = new PistolChamber(false);
            var magazine = new PistolMagazine(Rounds);
            pistol.AddChild(chamber);
            pistol.AddChild(magazine);
            var operation = RunAndWaitOperation(pistol);
            AssertPass(operation, Rounds, false, chamber, magazine);
        }
        
        [Test]
        public void EmptyMagazineFailTest()
        {
            var pistol = new Pistol();
            var chamber = new PistolChamber(false);
            var magazine = new PistolMagazine(0);
            pistol.AddChild(chamber);
            pistol.AddChild(magazine);
            var operation = RunAndWaitOperation(pistol);
            AssertFail(operation, 0, false, chamber, magazine);
        }
        
        [Test]
        public void NoMagazineFailTest()
        {
            var pistol = new Pistol();
            var chamber = new PistolChamber(false);
            pistol.AddChild(chamber);
            var operation = RunAndWaitOperation(pistol);
            AssertFail(operation, 0, false, chamber, null);
        }
        
        [Test]
        public void NoChamberFailTest()
        {
            var pistol = new Pistol();
            var magazine = new PistolMagazine(Rounds);
            pistol.AddChild(magazine);
            var operation = RunAndWaitOperation(pistol);
            AssertFail(operation, Rounds, false, null, magazine);
        }

        private static IOperation RunAndWaitOperation(Pistol pistol)
        {
            var world = UnitWorld.Create();
            var unit = world.GetOrCreateUnit(pistol);

            var identifier = OperationIdentifier.CreateNew();
            var operationUnit = new OperationUnit(unit);
            var operation = new WeaponShotUnitOperation(identifier, operationUnit, Array.Empty<IOperationMiddleware>());
            operation.RunOperation(world);
            world.UpdateUntilComplete(operation, Timeout);
            
            return operation;
        }
        
        private static void AssertPass(IOperation operation, int rounds, bool hasRound, PistolChamber? chamber, PistolMagazine? magazine)
        {
            if (operation.Exception != null)
                ExceptionDispatchInfo.Capture(operation.Exception).Throw();
            
            operation.AssertPass();
            
            if (chamber != null)
                Assert.AreEqual(false, chamber.HasRound);
            
            var exceptedRounds = hasRound ? rounds : rounds - 1;
            if (magazine != null)
                Assert.AreEqual(exceptedRounds, magazine.Rounds);
        }

        private static void AssertFail(IOperation operation, int rounds, bool hasRound, PistolChamber? chamber, PistolMagazine? magazine)
        {
            operation.AssertFail();
            
            if (chamber != null)
                Assert.AreEqual(hasRound, chamber.HasRound);
            
            if (magazine != null)
                Assert.AreEqual(rounds, magazine.Rounds);
        }
    }
}