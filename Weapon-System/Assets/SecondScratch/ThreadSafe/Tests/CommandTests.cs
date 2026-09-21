using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Unity.PerformanceTesting;

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

            var commandQueue = new AsyncCommandQueue();

            var processTask = Task.Run(ProcessCommands);

            foreach (var player in players)
                for (var j = 0; j < RepeatCount; j++)
                    commandQueue.Enqueue(player.CreateIncreaseHealthCommand(Value));

            await processTask;

            foreach (var player in players)
                Assert.AreEqual(Value * RepeatCount, player.Health);
            return;
            
            async Task ProcessCommands()
            {
                var exceptedCommandsCount = PlayerCount * RepeatCount;

                while (exceptedCommandsCount > 0)
                {
                    var command = await commandQueue.TryDequeue();
                    if (command == null)
                        continue;

                    command.Execute();
                    exceptedCommandsCount--;
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
            var players = new Player[PlayerCount];
            for (var i = 0; i < PlayerCount; i++)
                players[i] = new Player();
            var commandQueue = new AsyncCommandQueue();

            using (Measure.Scope())
            {
                var processTask = Task.Run(ProcessCommands);

                foreach (var player in players)
                    for (var j = 0; j < RepeatCount; j++)
                        commandQueue.Enqueue(player.CreateIncreaseHealthCommand(Value));

                await processTask;
            }
            
            foreach (var player in players)
            {
                Assert.AreEqual(Value * RepeatCount, player.Health);
                player.Reset();
            }
            return;
            
            async Task ProcessCommands()
            {
                var exceptedCommandsCount = PlayerCount * RepeatCount;

                while (exceptedCommandsCount > 0)
                {
                    var command = await commandQueue.TryDequeue();
                    if (command == null)
                        continue;

                    command.Execute();
                    exceptedCommandsCount--;
                }
            }
        }
    }

    public class AsyncCommandQueue
    {
        private readonly ConcurrentQueue<ICommand> _queue = new();
        private readonly SemaphoreSlim _signal = new(0);

        public void Enqueue(ICommand command)
        {
            _queue.Enqueue(command);
            _signal.Release();
        }

        public async ValueTask<ICommand?> TryDequeue(CancellationToken cancellationToken = default)
        {
            if (_queue.TryDequeue(out var item))
                return item;

            await _signal.WaitAsync(cancellationToken);

            if (_queue.TryDequeue(out item))
                return item;

            return null;
        }
    }

    public interface ICommand
    {
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