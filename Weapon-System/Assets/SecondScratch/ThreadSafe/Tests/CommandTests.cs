using System.Collections.Generic;
using NUnit.Framework;
using Unity.PerformanceTesting;

namespace SecondScratch.ThreadSafe.Tests
{
    public class CommandTests
    {
        public const int Value = 10;
        public const int RepeatCount = 100;
        
        public const int WarmupCount = 10;
        public const int MeasurementCount = 10;
        public const int IterationsPerMeasurement = 5;
        
        [Test]
        public void DirectCall_Test()
        {
            var player = new Player();

            for (var i = 0; i < RepeatCount; i++)
                player.IncreaseHealth(Value);
            
            Assert.AreEqual(Value * RepeatCount, player.Health);
        }

        [Test]
        public void Command_SingleThread_Test()
        {
            var player = new Player();

            var commandQueue = new Queue<Player.IncreaseHealthCommand>();

            for (var i = 0; i < RepeatCount; i++)
                commandQueue.Enqueue(player.CreateIncreaseHealthCommand(Value));

            while (commandQueue.TryDequeue(out var command))
                command.Execute();
            
            Assert.AreEqual(Value * RepeatCount, player.Health);
        }
        
        [Test]
        [Performance]
        public void DirectCall_Performance_Test()
        {
            var player = new Player();

            Measure.Method(Method)
                .CleanUp(CleanUp)
                .WarmupCount(WarmupCount)
                .MeasurementCount(MeasurementCount)
                .IterationsPerMeasurement(IterationsPerMeasurement)
                .GC()
                .Run();
            return;
            
            void Method()
            {
                for (var i = 0; i < RepeatCount; i++) 
                    player.IncreaseHealth(Value);
            }

            void CleanUp()
            {
                Assert.AreEqual(Value * RepeatCount, player.Health);
                player.Reset();
            }
        }
        
        [Test]
        [Performance]
        public void Command_SingleThread_Performance_Test()
        {
            var player = new Player();
            var commandQueue = new Queue<Player.IncreaseHealthCommand>();

            Measure.Method(Method)
                .CleanUp(CleanUp)
                .WarmupCount(WarmupCount)
                .MeasurementCount(MeasurementCount)
                .IterationsPerMeasurement(IterationsPerMeasurement)
                .GC()
                .Run();
            return;
            
            void Method()
            {
                for (var i = 0; i < RepeatCount; i++)
                    commandQueue.Enqueue(player.CreateIncreaseHealthCommand(Value));

                while (commandQueue.TryDequeue(out var command))
                    command.Execute();
            }

            void CleanUp()
            {
                Assert.AreEqual(Value * RepeatCount, player.Health);
                player.Reset();
                commandQueue.Clear();
            }
        }
    }

    public partial class Player
    {
        public int Health { get; private set; }

        public void IncreaseHealth(int value)
        {
            Health += value;
        }

        public void Reset()
        {
            Health = 0;
        }
    }
    
    //view
    public partial class Player
    {
        public View ToModel() => new(Health);
        
        public readonly struct View
        {
            public int Health { get; }
            
            public View(int health)
            {
                Health = health;
            }
        }
    }
    
    //command
    public partial class Player
    {
        public IncreaseHealthCommand CreateIncreaseHealthCommand(int value)
            => new IncreaseHealthCommand(this, value);
        
        public readonly struct IncreaseHealthCommand
        {
            private readonly Player _target;
            private readonly int _value;

            public IncreaseHealthCommand(Player target, int value)
            {
                _target = target;
                _value = value;
            }

            public void Execute()
            {
                _target.IncreaseHealth(_value);
            }
        }
    }
}