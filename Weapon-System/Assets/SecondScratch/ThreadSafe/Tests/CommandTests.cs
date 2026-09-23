using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Unity.PerformanceTesting;
using System.Threading.Channels;

namespace SecondScratch.ThreadSafe.Tests
{
    public class CommandTests
    {
        public const int Value = 10;
        public const int RepeatCount = 100;
        public const int PlayerCount = 100;

        public const int WarmupCount = 10;
        public const int MeasurementCount = 10;
        public const int IterationsPerMeasurement = 5;

        [Test]
        public void DirectCall_Test()
        {
            var players = new Player[PlayerCount];
            for (var i = 0; i < PlayerCount; i++)
                players[i] = new Player();

            foreach (var player in players)
                for (var j = 0; j < RepeatCount; j++)
                    player.IncreaseHealth(Value);

            foreach (var player in players)
                Assert.AreEqual(Value * RepeatCount, player.Health);
        }

        [Test]
        public async Task Command_SingleThread_Test()
        {
            var players = new Player[PlayerCount];
            for (var i = 0; i < PlayerCount; i++)
                players[i] = new Player();

            var commandChannel = Channel.CreateUnbounded<ICommand>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false,
                AllowSynchronousContinuations = true,
            });

            var processTask = Task.Run(ProcessCommands);

            foreach (var player in players)
                for (var j = 0; j < RepeatCount; j++)
                    commandChannel.Writer.TryWrite(player.CreateIncreaseHealthCommand(Value));

            await processTask;

            foreach (var player in players)
                Assert.AreEqual(Value * RepeatCount, player.Health);
            return;

            async ValueTask ProcessCommands()
            {
                var exceptedCommandsCount = PlayerCount * RepeatCount;

                while (exceptedCommandsCount > 0)
                {
                    while (commandChannel.Reader.TryRead(out var command))
                    {
                        command.Execute();
                        exceptedCommandsCount--;
                    }

                    await commandChannel.Reader.WaitToReadAsync();
                }
            }
        }

        [Test]
        [Performance]
        public void DirectCall_PerformanceTest()
        {
            var players = new Player[PlayerCount];
            for (var i = 0; i < PlayerCount; i++)
                players[i] = new Player();

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
                foreach (var player in players)
                    for (var j = 0; j < RepeatCount; j++)
                        player.IncreaseHealth(Value);
            }

            void CleanUp()
            {
                foreach (var player in players)
                {
                    Assert.AreEqual(Value * RepeatCount, player.Health);
                    player.Reset();
                }
            }
        }

        [Test]
        [Performance]
        public async Task Command_SingleThread_PerformanceTest()
        {
            for (var i = 0; i < WarmupCount; i++)
                await Command_SingleThread_Test();

            var players = new Player[PlayerCount];
            for (var i = 0; i < PlayerCount; i++)
                players[i] = new Player();

            var commandChannel = Channel.CreateUnbounded<ICommand>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false,
            });

            using (Measure.Scope())
            {
                var processTask = Task.Run(ProcessCommands);

                foreach (var player in players)
                    for (var j = 0; j < RepeatCount; j++)
                        commandChannel.Writer.TryWrite(player.CreateIncreaseHealthCommand(Value));

                await processTask;
            }

            foreach (var player in players)
            {
                Assert.AreEqual(Value * RepeatCount, player.Health);
                player.Reset();
            }

            return;

            async ValueTask ProcessCommands()
            {
                var exceptedCommandsCount = PlayerCount * RepeatCount;

                while (exceptedCommandsCount > 0)
                {
                    while (commandChannel.Reader.TryRead(out var command))
                    {
                        command.Execute();
                        exceptedCommandsCount--;
                    }

                    await commandChannel.Reader.WaitToReadAsync();
                }
            }
        }

        [Test]
        public async Task CommandScheduler_TargetLocalityAndBalance_Test()
        {
            const int channelCount = 4;
            const int commandsPerTarget = 200;

            using var scheduler = new CommandScheduler(channelCount);

            var targets = new[]
            {
                new Player(),
                new Player(),
                new Player(),
                new Player(),
            };

            for (var i = 0; i < commandsPerTarget; i++)
                foreach (var target in targets)
                    scheduler.ScheduleCommand(target.CreateIncreaseHealthCommand(Value));

            // Each target binds to its own channel while it still has pending commands.
            for (var channelIndex = 0; channelIndex < channelCount; channelIndex++)
                Assert.AreEqual(commandsPerTarget, scheduler.PendingCommandCount(channelIndex));

            for (var channelIndex = 0; channelIndex < channelCount; channelIndex++)
                await scheduler.DrainChannelAsync(channelIndex);

            foreach (var target in targets)
                Assert.AreEqual(commandsPerTarget * Value, target.Health);

            // Once drained, the target is rebound to the least-loaded channel.
            for (var i = 0; i < commandsPerTarget / 2; i++)
                scheduler.ScheduleCommand(targets[0].CreateIncreaseHealthCommand(Value));

            Assert.AreEqual(commandsPerTarget / 2, scheduler.PendingCommandCount(0));
            for (var channelIndex = 1; channelIndex < channelCount; channelIndex++)
                Assert.AreEqual(0, scheduler.PendingCommandCount(channelIndex));

            await scheduler.DrainChannelAsync(0);

            Assert.AreEqual((commandsPerTarget + commandsPerTarget / 2) * Value, targets[0].Health);
        }

        [Test]
        public async Task CommandScheduler_Multithreaded_Test()
        {
            const int playerCount = 16;
            const int producerCount = 6;
            const int commandsPerProducer = 500;

            using var scheduler = new CommandScheduler(4);
            scheduler.RunConsumers();

            var players = new Player[playerCount];
            for (var i = 0; i < playerCount; i++)
                players[i] = new Player();

            var scheduled = new int[playerCount];

            var producers = Enumerable.Range(0, producerCount)
                .Select(producerIndex => Task.Run(() =>
                {
                    for (var i = 0; i < commandsPerProducer; i++)
                    {
                        var playerIndex = (i + producerIndex) % playerCount;
                        scheduler.ScheduleCommand(players[playerIndex].CreateIncreaseHealthCommand(Value));
                        Interlocked.Increment(ref scheduled[playerIndex]);
                    }
                }))
                .ToArray();

            await Task.WhenAll(producers);

            var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(5);
            while (scheduler.TotalPendingCommands > 0 && DateTime.UtcNow < deadline)
                await Task.Delay(1);

            Assert.AreEqual(0, scheduler.TotalPendingCommands);

            for (var i = 0; i < playerCount; i++)
                Assert.AreEqual(scheduled[i] * Value, players[i].Health);
        }
    }

    public interface ICommand
    {
        object Target { get; }

        void Execute();
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
        public IncreaseHealthCommand CreateIncreaseHealthCommand(int value) => new(this, value);

        public readonly struct IncreaseHealthCommand : ICommand
        {
            private readonly Player _target;
            private readonly int _value;

            public object Target => _target;

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