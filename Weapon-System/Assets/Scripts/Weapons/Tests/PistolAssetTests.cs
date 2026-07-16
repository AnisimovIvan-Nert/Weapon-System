using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Weapons.Assets;
using Weapons.Operations;
using Weapons.Operations.AttachmentToggle;
using Weapons.Operations.Shot;
using Weapons.Tests.Mocks;
using Weapons.Units.Attachments;
using Weapons.Units.Weapons;
using Weapons.User;

namespace Weapons.Tests
{
    public class PistolAssetTests
    {
        private TestUser _user;
        private UnitAggregator _unitAggregator;

        [SetUp]
        public void SetUp()
        {
            var pistol = CreatePistol();
            pistol.Children.Add(CreateLaser());
            pistol.Children.Add(CreateLaser());
            
            _user = new TestUser();
            var userAdapter = new UserAdapter(_user);

            _unitAggregator = UnitAggregator.Create(userAdapter, pistol);
        }

        [Test]
        public async Task Test()
        {
            const int firstAttachment = 0;
            const int secondeAttachment = 1;
            
            _user.PressButton();
            
            _unitAggregator.Update();
            LogAssert.Expect(LogType.Log, WeaponController.Start);
            _user.Update();
            await Task.Yield();
            LogAssert.Expect(LogType.Log, WeaponController.Perform);
            
            _user.PressCancel();
            
            _unitAggregator.Update();
            LogAssert.Expect(LogType.Log, WeaponAnimator.Start);
            _user.Update();
            await Task.Yield();
            
            _unitAggregator.Update();
            LogAssert.Expect(LogType.Log, WeaponAnimator.Cancel);
            _user.Update();
            await Task.Yield();
            
            _unitAggregator.Update();
            LogAssert.Expect(LogType.Log, WeaponController.Cancel);
            _user.Update();
            await Task.Yield();
            
            
            _user.ToggleAttachments(secondeAttachment);
            
            _unitAggregator.Update();
            LogAssert.Expect(LogType.Log, AttachmentController.Start);
            LogAssert.Expect(LogType.Log, secondeAttachment.ToString());
            _user.Update();
            await Task.Yield();
            LogAssert.Expect(LogType.Log, AttachmentController.Perform);
            
            _user.PressCancel();
            
            _unitAggregator.Update();
            LogAssert.Expect(LogType.Log, AttachmentAnimator.Start);
            LogAssert.Expect(LogType.Log, secondeAttachment.ToString());
            _user.Update();
            await Task.Yield();
            
            _unitAggregator.Update();
            LogAssert.Expect(LogType.Log, AttachmentAnimator.Cancel);
            _user.Update();
            await Task.Yield();
            
            _unitAggregator.Update();
            LogAssert.Expect(LogType.Log, AttachmentController.Cancel);
            _user.Update();
            await Task.Yield();
            
            
            
            _user.ToggleAttachments();
            
            _unitAggregator.Update();
            LogAssert.Expect(LogType.Log, AttachmentController.Start);
            LogAssert.Expect(LogType.Log, firstAttachment.ToString());
            LogAssert.Expect(LogType.Log, AttachmentController.Start);
            LogAssert.Expect(LogType.Log, secondeAttachment.ToString());
            _user.Update();
            await Task.Yield();
            LogAssert.Expect(LogType.Log, AttachmentController.Perform);
            LogAssert.Expect(LogType.Log, AttachmentController.Perform);
            
            _user.PressCancel();
            
            _unitAggregator.Update();
            LogAssert.Expect(LogType.Log, AttachmentAnimator.Start);
            LogAssert.Expect(LogType.Log, firstAttachment.ToString());
            LogAssert.Expect(LogType.Log, AttachmentAnimator.Start);
            LogAssert.Expect(LogType.Log, secondeAttachment.ToString());
            _user.Update();
            await Task.Yield();
            
            _unitAggregator.Update();
            LogAssert.Expect(LogType.Log, AttachmentAnimator.Cancel);
            LogAssert.Expect(LogType.Log, AttachmentAnimator.Cancel);
            _user.Update();
            await Task.Yield();
            
            _unitAggregator.Update();
            LogAssert.Expect(LogType.Log, AttachmentController.Cancel);
            LogAssert.Expect(LogType.Log, AttachmentController.Cancel);
            _user.Update();
            await Task.Yield();
        }

        private static Laser CreateLaser()
        {
            var data = new UnitData(nameof(Laser));
            var controller = new AttachmentController();
            var animator = new AttachmentAnimator();
            var toggleOperationRunner = new AttachmentToggleOperationsRunner();

            var laser = new Laser(data, controller, animator);
            laser.OperationsRunners.Add(toggleOperationRunner);
            return laser;
        }
        
        private static Pistol CreatePistol()
        {
            var data = new UnitData(nameof(Pistol));
            var controller = new WeaponController();
            var animator = new WeaponAnimator();
            var shotOperationRunner = new ShotOperationsRunner();

            var pistol = new Pistol(data, controller, animator);
            pistol.OperationsRunners.Add(shotOperationRunner);
            return pistol;
        }
    }
}