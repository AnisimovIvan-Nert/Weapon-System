using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SecondScratch.ThreadSafe.Commands;
using SecondScratch.ThreadSafe.Schedulers.Implementations;
using SecondScratch.ThreadSafe.Tests.Mocks;

namespace SecondScratch.ThreadSafe.Tests
{
    public class CommandSchedulerTests
    {
        private const int Value = 10;

        [Test]
        public async Task MultiChanelCommandScheduler_TargetLocalityAndBalance_Test()
        {
            const int channelCount = 4;
            const int commandsPerTarget = 200;

            await using var scheduler = new MultiChannelCommandScheduler(channelCount);

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
                await scheduler.DrainAsync();

            foreach (var target in targets)
                Assert.AreEqual(commandsPerTarget * Value, target.Health);

            // Once drained, the target is rebound to the least-loaded channel.
            for (var i = 0; i < commandsPerTarget / 2; i++)
                scheduler.ScheduleCommand(targets[0].CreateIncreaseHealthCommand(Value));

            Assert.AreEqual(commandsPerTarget / 2, scheduler.PendingCommandCount(0));
            for (var channelIndex = 1; channelIndex < channelCount; channelIndex++)
                Assert.AreEqual(0, scheduler.PendingCommandCount(channelIndex));

            await scheduler.DrainAsync();

            Assert.AreEqual((commandsPerTarget + commandsPerTarget / 2) * Value, targets[0].Health);
        }

        [Test]
        public async Task MultiChanelCommandScheduler_Multithreaded_Test()
        {
            const int playerCount = 16;
            const int producerCount = 6;
            const int commandsPerProducer = 500;

            await using var scheduler = new MultiChannelCommandScheduler(4);
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

            var timeout = DateTime.UtcNow + TimeSpan.FromSeconds(5);
            while (scheduler.GetTotalPendingCommands() > 0 && DateTime.UtcNow < timeout)
                await Task.Delay(1);

            Assert.AreEqual(0, scheduler.GetTotalPendingCommands());

            for (var i = 0; i < playerCount; i++)
                Assert.AreEqual(scheduled[i] * Value, players[i].Health);
        }

        [Test]
        public async Task MultiChanelCommandScheduler_TargetAffinityAndStateRelease_Test()
        {
            const int channelCount = 4;
            const int playerCount = 32;
            const int producerCount = 6;
            const int commandsPerProducer = 1000;

            await using var scheduler = new MultiChannelCommandScheduler(channelCount);
            scheduler.RunConsumers();

            var players = new Player[playerCount];
            var probes = new TargetProbe[playerCount];
            for (var i = 0; i < playerCount; i++)
            {
                players[i] = new Player();
                probes[i] = new TargetProbe();
            }

            var sent = new int[playerCount];

            var producers = Enumerable.Range(0, producerCount)
                .Select(producerIndex => Task.Run(() =>
                {
                    for (var i = 0; i < commandsPerProducer; i++)
                    {
                        var playerIndex = (i * 7 + producerIndex) % playerCount;
                        scheduler.SendCommand(new ProbeCommand(probes[playerIndex], players[playerIndex]));
                        Interlocked.Increment(ref sent[playerIndex]);
                    }
                }))
                .ToArray();

            await Task.WhenAll(producers);

            var timeout = DateTime.UtcNow + TimeSpan.FromSeconds(10);
            while (scheduler.GetTotalPendingCommands() > 0 && DateTime.UtcNow < timeout)
                await Task.Delay(1);

            Assert.AreEqual(0, scheduler.GetTotalPendingCommands());

            for (var i = 0; i < playerCount; i++)
            {
                Assert.AreEqual(1, probes[i].MaxConcurrency, $"target {i} ran commands on two channels at once");
                Assert.AreEqual(sent[i] * Value, players[i].Health);
            }

            Assert.AreEqual(0, scheduler.RegisteredTargetStateCount);
        }

        private sealed class TargetProbe
        {
            private int _concurrent;
            private int _maxConcurrency;

            public int MaxConcurrency => Volatile.Read(ref _maxConcurrency);

            public void Enter()
            {
                var concurrent = Interlocked.Increment(ref _concurrent);

                var max = Volatile.Read(ref _maxConcurrency);
                while (concurrent > max)
                {
                    var previous = Interlocked.CompareExchange(ref _maxConcurrency, concurrent, max);
                    if (previous == max)
                        return;

                    max = previous;
                }
            }

            public void Exit() => Interlocked.Decrement(ref _concurrent);
        }

        private sealed class ProbeCommand : ICommand
        {
            private readonly TargetProbe _probe;
            private readonly Player _target;

            public ProbeCommand(TargetProbe probe, Player target)
            {
                _probe = probe;
                _target = target;
            }

            public object Target => _target;

            public void Execute()
            {
                _probe.Enter();
                try
                {
                    _target.IncreaseHealth(Value);
                }
                finally
                {
                    _probe.Exit();
                }
            }
        }

        [Test]
        public async Task ChanelCommandScheduler_TargetLocalityAndBalance_Test()
        {
            const int commandsPerTarget = 200;

            await using var scheduler = new ChannelCommandScheduler();

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

            Assert.AreEqual(commandsPerTarget * targets.Length, scheduler.GetTotalPendingCommands());
            
            await scheduler.DrainAsync();

            foreach (var target in targets)
                Assert.AreEqual(commandsPerTarget * Value, target.Health);
        }

        [Test]
        public async Task ChanelCommandScheduler_Multithreaded_Test()
        {
            const int playerCount = 16;
            const int producerCount = 6;
            const int commandsPerProducer = 500;

            await using var scheduler = new ChannelCommandScheduler();
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

            var timeout = DateTime.UtcNow + TimeSpan.FromSeconds(5);
            while (scheduler.GetTotalPendingCommands() > 0 && DateTime.UtcNow < timeout)
                await Task.Delay(1);

            Assert.AreEqual(0, scheduler.GetTotalPendingCommands());

            for (var i = 0; i < playerCount; i++)
                Assert.AreEqual(scheduled[i] * Value, players[i].Health);
        }
    }
}