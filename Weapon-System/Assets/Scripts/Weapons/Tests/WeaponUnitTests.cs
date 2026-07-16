using NUnit.Framework;
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
            _userAdapter.Update();
            _weapon.Update();
            
            _user.PressCancel();
            
            _user.Update();
            _userAdapter.Update();
            _weapon.Update();
            
            _user.Update();
            _userAdapter.Update();
            _weapon.Update();
            
            _user.Update();
            _userAdapter.Update();
            _weapon.Update();
            
            _user.Update();
            _userAdapter.Update();
            _weapon.Update();
            
            _user.Update();
            _userAdapter.Update();
            _weapon.Update();
            
            _user.Update();
            _userAdapter.Update();
            _weapon.Update();
        }
    }
}