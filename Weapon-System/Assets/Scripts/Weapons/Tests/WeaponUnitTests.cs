using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Weapons.Operations;
using Weapons.Operations.Shot;
using Weapons.Tests.Mocks;
using Weapons.Units.Weapons;
using Weapons.User;

namespace Weapons.Tests
{
    public class WeaponsTests
    {
        private TestUser _user;
        private Weapon _weapon;

        [SetUp]
        public void SetUp()
        {
            var operationsRunner = new ShotOperationsRunner();
            var data = new UnitData(nameof(WeaponsTests));

            var controller = new WeaponController();
            var animator = new WeaponAnimator();
            
            _user = new TestUser();
            var userAdapter = new UserAdapter(_user);

            _weapon = new Weapon(userAdapter, data, controller, animator, new[] { operationsRunner });
        }

        [Test]
        public async Task Test()
        {
            _user.PressButton();
            
            _weapon.Update();
            LogAssert.Expect(LogType.Log, WeaponController.Start);
            _user.Update();
            await Task.Yield();
            LogAssert.Expect(LogType.Log, WeaponController.Perform);
            
            _user.PressCancel();
            
            _weapon.Update();
            LogAssert.Expect(LogType.Log, WeaponAnimator.Start);
            _user.Update();
            await Task.Yield();
            
            _weapon.Update();
            LogAssert.Expect(LogType.Log, WeaponAnimator.Cancel);
            _user.Update();
            await Task.Yield();
            
            _weapon.Update();
            LogAssert.Expect(LogType.Log, WeaponController.Cancel);
            _user.Update();
            await Task.Yield();
        }
    }
}