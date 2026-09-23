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
        private const int RepeatCount = 100;
        private const int PlayerCount = 100;

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

                    await commandChannel.Reader.WaitToReadAsync().ConfigureAwait(false);
                }
            }
        }
        
        [Test]
        public async Task Command_MultiThread_Test()
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
    }
}