using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Unity.PerformanceTesting;
using System.Threading.Channels;
using SecondScratch.ThreadSafe.Commands;
using SecondScratch.ThreadSafe.Schedulers.Implementations;
using SecondScratch.ThreadSafe.Tests.Mocks;

namespace SecondScratch.ThreadSafe.Tests
{
    public class CommandTests
    {
        private const int Value = 10;
        private const int RepeatCount = 1024;
        private const int PlayerCount = 1024;
        private const int ProducerCount = 16;
        private const int ChannelCount = 16;

        private const int WarmupCount = 10;
        private const int MeasurementCount = 10;
        private const int IterationsPerMeasurement = 5;

        private const string ScheduleCommandsScope = "Schedule commands";
        private const string ExecuteCommandsScope = "Execute commands";

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

            commandChannel.Writer.TryComplete();

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

            ValueTask? lastTask = null;
            foreach (var player in players)
            {
                for (var j = 0; j < RepeatCount; j++)
                {
                    var command = player.CreateIncreaseHealthCommand(Value);
                    lastTask = scheduler.ScheduleCommand(command);
                }
            }

            if (lastTask != null)
                await lastTask.Value;

            Assert.AreEqual(0, scheduler.GetTotalPendingCommands());

            foreach (var player in players)
                Assert.AreEqual(Value * RepeatCount, player.Health);
        }

        [Test]
        public async Task Command_MultiChannelCommandScheduler_Test()
        {
            const int playerPerProducer = PlayerCount / ProducerCount;
            Assert.AreEqual(PlayerCount, playerPerProducer * ProducerCount);

            var players = new Player[PlayerCount];
            for (var i = 0; i < PlayerCount; i++)
                players[i] = new Player();

            await using var scheduler = new MultiChannelCommandScheduler(ChannelCount);
            scheduler.RunConsumers();

            var producers = Enumerable.Range(0, ProducerCount)
                .Select(producerIndex => Task.Run(async () =>
                {
                    var lastTasks = new ValueTask[playerPerProducer];
                    var taskIndex = 0;

                    var start = producerIndex * playerPerProducer;
                    var end = start + playerPerProducer;
                    for (var i = start; i < end; i++)
                    {
                        for (var j = 0; j < RepeatCount; j++)
                        {
                            var command = players[i].CreateIncreaseHealthCommand(Value);
                            if (j == RepeatCount - 1)
                                lastTasks[taskIndex++] = scheduler.ScheduleCommand(command);
                            else
                                scheduler.SendCommand(command);
                        }
                    }

                    foreach (var lastTask in lastTasks)
                        await lastTask;
                }))
                .ToArray();

            await Task.WhenAll(producers);

            Assert.AreEqual(0, scheduler.GetTotalPendingCommands());

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
            for (var warmup = 0; warmup < WarmupCount; warmup++)
            {
                var players = new Player[PlayerCount];
                for (var i = 0; i < PlayerCount; i++)
                    players[i] = new Player();

                var commandChannel = Channel.CreateUnbounded<ICommand>(new UnboundedChannelOptions
                {
                    SingleReader = true,
                    SingleWriter = false,
                    AllowSynchronousContinuations = true
                });

                using (Measure.Scope(ScheduleCommandsScope))
                {
                    foreach (var player in players)
                        for (var j = 0; j < RepeatCount; j++)
                            commandChannel.Writer.TryWrite(player.CreateIncreaseHealthCommand(Value));
                }

                using (Measure.Scope(ExecuteCommandsScope))
                {
                    var processTask = Task.Run(() => ProcessCommands(commandChannel));
                    await processTask;
                }

                commandChannel.Writer.TryComplete();

                foreach (var player in players)
                {
                    Assert.AreEqual(Value * RepeatCount, player.Health);
                    player.Reset();
                }
            }

            return;

            async ValueTask ProcessCommands(Channel<ICommand> channel)
            {
                var exceptedCommandsCount = PlayerCount * RepeatCount;

                while (exceptedCommandsCount > 0)
                {
                    while (channel.Reader.TryRead(out var command))
                    {
                        command.Execute();
                        exceptedCommandsCount--;
                    }

                    await channel.Reader.WaitToReadAsync();
                }
            }
        }

        [Test]
        [Performance]
        public async Task Command_ChannelCommandScheduler_PerformanceTest()
        {
            await using var scheduler = new ChannelCommandScheduler();

            for (var warmup = 0; warmup < WarmupCount + 1; warmup++)
            {
                var players = new Player[PlayerCount];
                for (var i = 0; i < PlayerCount; i++)
                    players[i] = new Player();

                ValueTask? lastTask = null;
                using (Measure.Scope(ScheduleCommandsScope))
                {
                    for (var i = 0; i < players.Length; i++)
                    {
                        var player = players[i];
                        for (var j = 0; j < RepeatCount; j++)
                        {
                            var command = player.CreateIncreaseHealthCommand(Value);
                            if (i + 1 == players.Length && j + 1 == RepeatCount)
                                lastTask = scheduler.ScheduleCommand(command);
                            else
                                scheduler.SendCommand(command);
                        }
                    }
                }

                using (Measure.Scope(ExecuteCommandsScope))
                {
                    scheduler.RunConsumers();
                    if (lastTask != null)
                        await lastTask.Value;
                }

                scheduler.KillConsumers();

                Assert.AreEqual(0, scheduler.GetTotalPendingCommands());

                foreach (var player in players)
                    Assert.AreEqual(Value * RepeatCount, player.Health);
            }
        }

        [Test]
        [Performance]
        public async Task Command_MultiChannelCommandScheduler_PerformanceTest()
        {
            const int playerPerProducer = PlayerCount / ProducerCount;
            Assert.AreEqual(PlayerCount, playerPerProducer * ProducerCount);

            await using var scheduler = new MultiChannelCommandScheduler(ChannelCount);

            for (var warmup = 0; warmup < WarmupCount + 1; warmup++)
            {
                var players = new Player[PlayerCount];
                for (var i = 0; i < PlayerCount; i++)
                    players[i] = new Player();

                var lastTasks = new ValueTask[PlayerCount];

                using (Measure.Scope(ScheduleCommandsScope))
                {
                    var producers = Enumerable.Range(0, ProducerCount)
                        .Select(producerIndex => Task.Run(() =>
                        {
                            var start = producerIndex * playerPerProducer;
                            var end = start + playerPerProducer;
                            for (var i = start; i < end; i++)
                            {
                                for (var j = 0; j < RepeatCount; j++)
                                {
                                    var command = players[i].CreateIncreaseHealthCommand(Value);
                                    if (j == RepeatCount - 1)
                                        lastTasks[i] = scheduler.ScheduleCommand(command);
                                    else
                                        scheduler.SendCommand(command);
                                }
                            }
                        }))
                        .ToArray();

                    await Task.WhenAll(producers);
                }

                using (Measure.Scope(ExecuteCommandsScope))
                {
                    scheduler.RunConsumers();
                    await Task.WhenAll(lastTasks.Select(o => o.AsTask()));
                }

                scheduler.KillConsumers();

                Assert.AreEqual(0, scheduler.GetTotalPendingCommands());

                foreach (var player in players)
                    Assert.AreEqual(Value * RepeatCount, player.Health);
            }
        }
    }
}