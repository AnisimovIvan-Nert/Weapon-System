using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
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
            while (scheduler.TotalPendingCommands > 0 && DateTime.UtcNow < timeout)
                await Task.Delay(1);

            Assert.AreEqual(0, scheduler.TotalPendingCommands);

            for (var i = 0; i < playerCount; i++)
                Assert.AreEqual(scheduled[i] * Value, players[i].Health);
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

            Assert.AreEqual(commandsPerTarget * targets.Length, scheduler.TotalPendingCommands);
            
            await scheduler.DrainChannelAsync();

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
            while (scheduler.TotalPendingCommands > 0 && DateTime.UtcNow < timeout)
                await Task.Delay(1);

            Assert.AreEqual(0, scheduler.TotalPendingCommands);

            for (var i = 0; i < playerCount; i++)
                Assert.AreEqual(scheduled[i] * Value, players[i].Health);
        }
    }
}