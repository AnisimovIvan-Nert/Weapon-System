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
        private const int RepeatCount = 1024;
        private const int PlayerCount = 1000;
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

            ICommand? lastCommand = null;
            foreach (var player in players)
            {
                for (var j = 0; j < RepeatCount; j++)
                {
                    lastCommand = player.CreateIncreaseHealthCommand(Value);
                    scheduler.ScheduleCommand(lastCommand);
                }
            }

            if (lastCommand != null)
                await lastCommand.ExecutionTask;

            Assert.AreEqual(0, scheduler.TotalPendingCommands);

            foreach (var player in players)
                Assert.AreEqual(Value * RepeatCount, player.Health);
        }

        [Test]
        public async Task Command_MultiChannelCommandScheduler_Test()
        {
            const int repeatCount = RepeatCount / ProducerCount;
            Assert.AreEqual(RepeatCount, repeatCount * ProducerCount);

            var players = new Player[PlayerCount];
            for (var i = 0; i < PlayerCount; i++)
                players[i] = new Player();
            var lastCommands = new ICommand[ProducerCount];

            await using var scheduler = new MultiChannelCommandScheduler(ChannelCount);
            scheduler.RunConsumers();

            var producers = Enumerable.Range(0, ProducerCount)
                .Select(producerIndex => Task.Run(() =>
                {
                    for (var playerIndex = 0; playerIndex < PlayerCount; playerIndex++)
                    {
                        for (var i = 0; i < repeatCount; i++)
                        {
                            lastCommands[producerIndex] = players[playerIndex].CreateIncreaseHealthCommand(Value);
                            scheduler.ScheduleCommand(lastCommands[producerIndex]);
                        }
                    }
                }))
                .ToArray();

            await Task.WhenAll(producers);

            await Task.WhenAll(lastCommands.Select(command => command.ExecutionTask).ToArray());

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
            for (var warmup = 0; warmup < WarmupCount + 1; warmup++)
            {
                var players = new Player[PlayerCount];
                for (var i = 0; i < PlayerCount; i++)
                    players[i] = new Player();

                await using var scheduler = new ChannelCommandScheduler();

                ICommand? lastCommand = null;

                using (Measure.Scope(ScheduleCommandsScope))
                {
                    foreach (var player in players)
                    {
                        for (var j = 0; j < RepeatCount; j++)
                        {
                            lastCommand = player.CreateIncreaseHealthCommand(Value);
                            scheduler.ScheduleCommand(lastCommand);
                        }
                    }
                }

                using (Measure.Scope(ExecuteCommandsScope))
                {
                    scheduler.RunConsumers();
                    if (lastCommand != null)
                        await lastCommand.ExecutionTask;
                }

                Assert.AreEqual(0, scheduler.TotalPendingCommands);

                foreach (var player in players)
                    Assert.AreEqual(Value * RepeatCount, player.Health);
            }
        }

        [Test]
        [Performance]
        public async Task Command_MultiChannelCommandScheduler_PerformanceTest()
        {
            const int repeatCount = RepeatCount / ProducerCount;
            Assert.AreEqual(RepeatCount, repeatCount * ProducerCount);

            for (var warmup = 0; warmup < WarmupCount + 1; warmup++)
            {
                var players = new Player[PlayerCount];
                for (var i = 0; i < PlayerCount; i++)
                    players[i] = new Player();
                var lastCommands = new ICommand[ProducerCount];

                await using var scheduler = new ChannelCommandScheduler();

                using (Measure.Scope(ScheduleCommandsScope))
                {
                    var producers = Enumerable.Range(0, ProducerCount)
                        .Select(producerIndex => Task.Run(() =>
                        {
                            for (var playerIndex = 0; playerIndex < PlayerCount; playerIndex++)
                            {
                                for (var i = 0; i < repeatCount; i++)
                                {
                                    lastCommands[producerIndex] =
                                        players[playerIndex].CreateIncreaseHealthCommand(Value);
                                    scheduler.ScheduleCommand(lastCommands[producerIndex]);
                                }
                            }
                        }))
                        .ToArray();

                    await Task.WhenAll(producers);
                }

                using (Measure.Scope(ExecuteCommandsScope))
                {
                    scheduler.RunConsumers();
                    await Task.WhenAll(lastCommands.Select(command => command.ExecutionTask).ToArray());
                }

                Assert.AreEqual(0, scheduler.TotalPendingCommands);

                foreach (var player in players)
                    Assert.AreEqual(Value * RepeatCount, player.Health);
            }
        }
    }
}