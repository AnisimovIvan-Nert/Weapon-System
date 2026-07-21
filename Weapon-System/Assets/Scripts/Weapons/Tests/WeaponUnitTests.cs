using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Weapons.Operations;
using Weapons.ProducerConsumer;
using Weapons.ProducerConsumer.Consumers;
using Weapons.ProducerConsumer.Producers;
using Weapons.Tests.Mocks;
using Weapons.Units.Weapons;
using Weapons.Units.Weapons.Controller;

namespace Weapons.Tests
{
    public class WeaponsTests
    {
        private TestInput _input;
        private IEventProducer _eventProducer;
        private IOperationRunner _operationRunner;
        private Weapon _weapon;

        [SetUp]
        public void SetUp()
        {
            _operationRunner = new OperationRunner();
            
            _input = new TestInput();
            _eventProducer = new EventProducerAggregator(
                new WeaponTriggerEventProducer(_input), 
                new CancellationEventProducer(_input));
            
            var triggerConsumer = new WeaponTriggerEventConsumer(_eventProducer);
            var data = new WeaponData(nameof(WeaponsTests));

            var controller = new WeaponController();
            var animator = new WeaponAnimator();

            _weapon = new Weapon(_eventProducer, _operationRunner, data, controller, animator, new[] { triggerConsumer });
        }

        [Test]
        public void Test()
        {
            _input.PressShoot();
            _eventProducer.Update(); //create shot event
            _weapon.Update();
            _operationRunner.Update(); //handle event => create operation => controller start => yield null
            LogAssert.Expect(LogType.Log, WeaponController.Start);
            
            _input.Update();
            _eventProducer.Update();
            _weapon.Update();
            _operationRunner.Update(); //controller perform => animator start => yield null
            LogAssert.Expect(LogType.Log, WeaponController.Perform);
            LogAssert.Expect(LogType.Log, WeaponAnimator.Start);
            
            _input.Update();
            _input.PressCancel();
            _eventProducer.Update(); //create cancel event
            _weapon.Update();
            _operationRunner.Update(); //animator perform => yield null (by animator cancel)
            LogAssert.Expect(LogType.Log, WeaponAnimator.Perform);
            
            _input.Update();
            _eventProducer.Update();
            _weapon.Update();
            _operationRunner.Update(); //animator cancel => yield null (by controller cancel)
            LogAssert.Expect(LogType.Log, WeaponAnimator.Cancel);
            
            _input.Update();
            _eventProducer.Update();
            _weapon.Update();
            Assert.Throws<Exception>(() => _operationRunner.Update()); //controller cancel => handle operation result => throw exception
            LogAssert.Expect(LogType.Log, WeaponController.Cancel);
        }
    }
}