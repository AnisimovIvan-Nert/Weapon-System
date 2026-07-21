using System;
using NUnit.Framework;
using UnityEngine.TestTools;
using Weapons.Assets;
using Weapons.Operations;
using Weapons.Operations.Implementation.Weapons;
using Weapons.UnitHandlers.Implementation;
using Weapons.Units;
using Weapons.Units.Implementations;

namespace Weapons.Tests
{
    public class ShotTests
    {
        private const int Timeout = 100;

        private const int Rounds = 10;
        
        [Test]
        public void LoadedChamberPassTest()
        {
            var pistol = new Pistol();
            pistol.Children.Add(new PistolChamber(true));
            var (operation, weapon) = RunAndWaitOperation(pistol);
            AssertPass(operation, weapon, 0, true, true, false);
        }
        
        [Test]
        public void EmptyChamberPassTest()
        {
            var pistol = new Pistol();
            pistol.Children.Add(new PistolChamber(false));
            pistol.Children.Add(new PistolMagazine(Rounds));
            var (operation, weapon) = RunAndWaitOperation(pistol);
            AssertPass(operation, weapon, Rounds, false, true, true);
        }
        
        [Test]
        public void EmptyMagazineFailTest()
        {
            var pistol = new Pistol();
            pistol.Children.Add(new PistolChamber(false));
            pistol.Children.Add(new PistolMagazine(0));
            var (operation, weapon) = RunAndWaitOperation(pistol);
            AssertFail(operation, weapon, 0, false, true, true);
        }
        
        [Test]
        public void NoMagazineFailTest()
        {
            var pistol = new Pistol();
            pistol.Children.Add(new PistolChamber(false));
            var (operation, weapon) = RunAndWaitOperation(pistol);
            AssertFail(operation, weapon, 0, false, true, false);
        }
        
        [Test]
        public void NoChamberFailTest()
        {
            var pistol = new Pistol();
            pistol.Children.Add(new PistolMagazine(Rounds));
            var (operation, weapon) = RunAndWaitOperation(pistol);
            AssertFail(operation, weapon, Rounds, false, false, true);
        }

        private static (IOperation, IWeapon) RunAndWaitOperation(Pistol pistol)
        {
            var unit = (IWeapon)pistol.ToUnit();
            var operationRunner = new OperationRunner();

            var handler = new WeaponHandler(unit, operationRunner);

            var operation = new WeaponShotOperation(Guid.NewGuid());
            operation.RunOperation(handler);

            var timeout = Timeout;
            while (!operation.IsCompleted && timeout > 0)
            {
                timeout--;
                handler.Update();
            }

            return (operation, unit);
        }
        
        private static void AssertPass(IOperation operation, IWeapon weapon, int rounds, bool hasRound, bool hasChamber, bool hasMagazine)
        {
            LogAssert.Expect("Shot");
            
            if (!operation.IsCompleted)
                Assert.Fail();
            
            if (hasChamber)
            {
                var chamber = weapon.TryFind<IChamber>();
                Assert.NotNull(chamber);
                Assert.AreEqual(false, chamber!.HasRound);
            }
            
            if (hasMagazine)
            {
                var exceptedRounds = hasRound ? rounds : rounds - 1;
                var magazine = weapon.TryFind<IMagazine>();
                Assert.NotNull(magazine);
                Assert.AreEqual(exceptedRounds, magazine!.Rounds);
            }
            
            if (operation.IsCompletedSuccessfully)
                Assert.Pass();
                
            if (operation.Exception != null)
                Assert.Fail();
                
            Assert.Fail();
        }

        private static void AssertFail(IOperation operation, IWeapon weapon, int rounds, bool hasRound, bool hasChamber, bool hasMagazine)
        {
            if (!operation.IsCompleted)
                Assert.Fail();
            
            if (operation.IsCompletedSuccessfully)
                Assert.Fail();

            if (hasChamber)
            {
                var chamber = weapon.TryFind<IChamber>();
                Assert.NotNull(chamber);
                Assert.AreEqual(hasRound, chamber!.HasRound);
            }
            
            if (hasMagazine)
            {
                var magazine = weapon.TryFind<IMagazine>();
                Assert.NotNull(magazine);
                Assert.AreEqual(rounds, magazine!.Rounds);
            }
                
            if (operation.Exception != null)
                Assert.Pass();
                
            Assert.Fail();
        }
    }
}