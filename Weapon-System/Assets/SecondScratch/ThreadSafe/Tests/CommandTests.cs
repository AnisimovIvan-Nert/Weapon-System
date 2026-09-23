using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Unity.PerformanceTesting;
using System.Threading.Channels;
using SecondScratch.ThreadSafe.Tests.Mocks;

namespace SecondScratch.ThreadSafe.Tests
{
    public class CommandTests
    {
        private const int Value = 10;
        private const int RepeatCount = 128;
        private const int PlayerCount = 100;
        private const int ProducerCount = 8;
        private const int ChannelCount = 4;

        private const int WarmupCount = 10;
        private const int MeasurementCount = 10;
        private const int IterationsPerMeasurement = 5;

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
        public async Task Command_RawChannel_Test()
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

                    await commandChannel.Reader.WaitToReadAsync().ConfigureAwait(false);
                }
            }
        }

        [Test]
        public async Task Command_ChannelCommandScheduler_Test()
        {
            var players = new Player[PlayerCount];
            for (var i = 0; i < PlayerCount; i++)
                players[i] = new Player();
            
            await using var scheduler = new ChannelCommandScheduler();
            scheduler.RunConsumers();

            foreach (var player in players)
                for (var j = 0; j < RepeatCount; j++)
                    scheduler.ScheduleCommand(player.CreateIncreaseHealthCommand(Value));
            
            var timeout = DateTime.UtcNow + TimeSpan.FromSeconds(5);
            while (scheduler.TotalPendingCommands > 0 && DateTime.UtcNow < timeout)
                await Task.Yield();
            
            Assert.AreEqual(0, scheduler.TotalPendingCommands);

            foreach (var player in players)
                Assert.AreEqual(Value * RepeatCount, player.Health);
        }
        
        [Test]
        public async Task Command_MultiThread_Test()
        {
            const int repeatCount = RepeatCount / ProducerCount;
            Assert.AreEqual(RepeatCount, repeatCount * ProducerCount);
            
            var players = new Player[PlayerCount];
            for (var i = 0; i < PlayerCount; i++)
                players[i] = new Player();
            
            await using var scheduler = new MultiChannelCommandScheduler(ChannelCount);
            scheduler.RunConsumers();
            
            var producers = Enumerable.Range(0, ProducerCount)
                .Select(_ => Task.Run(() =>
                {
                    for (var playerIndex = 0; playerIndex < PlayerCount; playerIndex++)
                        for (var i = 0; i < repeatCount; i++)
                            scheduler.ScheduleCommand(players[playerIndex].CreateIncreaseHealthCommand(Value));
                }))
                .ToArray();

            await Task.WhenAll(producers);

            var timeout = DateTime.UtcNow + TimeSpan.FromSeconds(5);
            while (scheduler.TotalPendingCommands > 0 && DateTime.UtcNow < timeout)
                await Task.Yield();
            
            Assert.AreEqual(0, scheduler.TotalPendingCommands);

            foreach (var player in players)
                Assert.AreEqual(Value * RepeatCount, player.Health);
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
        public async Task Command_RawChannel_PerformanceTest()
        {
            for (var i = 0; i < WarmupCount; i++)
                await Command_RawChannel_Test();

            var players = new Player[PlayerCount];
            for (var i = 0; i < PlayerCount; i++)
                players[i] = new Player();

            var commandChannel = Channel.CreateUnbounded<ICommand>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false,
                AllowSynchronousContinuations = true
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
        [Performance]
        public async Task Command_ChannelCommandScheduler_PerformanceTest()
        {
            for (var i = 0; i < WarmupCount; i++)
                await Command_ChannelCommandScheduler_Test();

            var players = new Player[PlayerCount];
            for (var i = 0; i < PlayerCount; i++)
                players[i] = new Player();

            await using var scheduler = new ChannelCommandScheduler();
            scheduler.RunConsumers();

            using (Measure.Scope())
            {
                foreach (var player in players)
                    for (var j = 0; j < RepeatCount; j++)
                        scheduler.ScheduleCommand(player.CreateIncreaseHealthCommand(Value));
                
                while (scheduler.TotalPendingCommands > 0)
                    await Task.Yield();
            }
            
            Assert.AreEqual(0, scheduler.TotalPendingCommands);

            foreach (var player in players)
            {
                Assert.AreEqual(Value * RepeatCount, player.Health);
                player.Reset();
            }
        }
    }
}