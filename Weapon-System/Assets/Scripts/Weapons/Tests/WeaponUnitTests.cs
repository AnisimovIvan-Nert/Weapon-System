using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Weapons.Operations.Shot;
using Weapons.Tests.Mocks;
using Weapons.Units.Weapons;
using Weapons.User;

namespace Weapons.Tests
{
    public class WeaponsTests
    {
        private TestUser _user;
        private UserAdapter _userAdapter;
        private Weapon _weapon;

        [SetUp]
        public void SetUp()
        {
            var operationsRunner = new ShotOperationsRunner();
            var data = new WeaponData(nameof(WeaponsTests));

            var controller = new WeaponController();
            var animator = new WeaponAnimator();
            
            _user = new TestUser();
            _userAdapter = new UserAdapter(_user);

            _weapon = new Weapon(_userAdapter, data, controller, animator, new[] { operationsRunner });
        }

        [Test]
        public void Test()
        {
            _user.PressButton();
            _userAdapter.Update(); //create shot event
            _weapon.Update(); //handle event => create operation => controller start => yield null
            LogAssert.Expect(LogType.Log, WeaponController.Start);
            
            _user.Update();
            _userAdapter.Update();
            _weapon.Update(); //controller perform => animator start => yield null
            LogAssert.Expect(LogType.Log, WeaponController.Perform);
            LogAssert.Expect(LogType.Log, WeaponAnimator.Start);
            
            _user.Update();
            _user.PressCancel();
            _userAdapter.Update(); //create cancel event
            _weapon.Update(); //animator perform => yield null (by animator cancel)
            LogAssert.Expect(LogType.Log, WeaponAnimator.Perform);
            
            _user.Update();
            _userAdapter.Update();
            _weapon.Update(); //animator cancel => yield null (by controller cancel)
            LogAssert.Expect(LogType.Log, WeaponAnimator.Cancel);
            
            _user.Update();
            _userAdapter.Update();
            Assert.Throws<Exception>(() => _weapon.Update()); //controller cancel => handle operation result => throw exception
            LogAssert.Expect(LogType.Log, WeaponController.Cancel);
        }
    }
}