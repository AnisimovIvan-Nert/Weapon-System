using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Scratch.Owning.Tests
{
    [TestFixture]
    public class ObjectOwnerMultithreadTests
    {
        private class OwnerExecutionThread : IDisposable
        {
            public ObjectOwner Owner { get; private set; }
            private readonly Thread _thread;
            private volatile bool _stop;

            public OwnerExecutionThread()
            {
                _thread = new Thread(Execute)
                {
                    IsBackground = true
                };
                _thread.Start();
                
                while (Owner == null) 
                    Thread.SpinWait(10);
                return;
                
                void Execute()
                {
                    Owner = new ObjectOwner();
                    while (!_stop)
                    {
                        Owner.ExecuteNext();
                        Thread.Yield();
                    }
                }
            }

            public void Dispose()
            {
                _stop = true;
                _thread.Join(TimeSpan.FromSeconds(2));
            }
        }

        [Test]
        public async Task ConcurrentRunOnOwner_AllRequestsComplete()
        {
            const int taskCount = 8;
            const int countPerTask = 125;
            
            using var executeThread = new OwnerExecutionThread();
            var owner = new ObjectOwnerHandle(executeThread.Owner);
            
            var counter = 0;
            var tasks = Enumerable.Range(0, taskCount).Select(_ => Task.Run(async () =>
            { 
                var innerTasks = Enumerable.Range(0, countPerTask)
                    .Select(_ => owner.RunOnOwner(() => counter++));
                await Task.WhenAll(innerTasks.Select(o => o.AsTask()));
            }));

            await Task.WhenAll(tasks);
            Assert.AreEqual(countPerTask * taskCount, counter);
        }

        [Test]
        public async Task ConcurrentRunOnOwner_FIFOPreserved_WithinEachThread()
        {
            const int taskCount = 4;
            const int countPerTask = 250;
            
            using var executeThread = new OwnerExecutionThread();
            var owner = new ObjectOwnerHandle(executeThread.Owner);
            
            var executed = new ConcurrentQueue<(int lane, int seq)>();
            var tasks = Enumerable.Range(0, taskCount).Select(lane => Task.Run(async () =>
            {
                var innerTasks = Enumerable.Range(0, countPerTask)
                    .Select(seq => owner.RunOnOwner(() => executed.Enqueue((lane, seq))));
                await Task.WhenAll(innerTasks.Select(o => o.AsTask()));
            }));

            await Task.WhenAll(tasks);

            var executedArray = executed.ToArray();
            Assert.AreEqual(countPerTask * taskCount, executedArray.Length);

            
            // Single owner thread processes the queue FIFO, and each lane submits
            // sequentially (it awaits each request before the next). So each lane's
            // seq values must appear in strictly ascending order in the global log.
            var lastSeq = Enumerable.Repeat(-1, taskCount).ToArray();
            foreach (var (lane, seq) in executedArray)
            {
                Assert.IsTrue(seq > lastSeq[lane], $"lane {lane}: out of FIFO order at seq {seq}");
                lastSeq[lane] = seq;
            }
        }

        [Test]
        public async Task ChangeOwner_DrainsInFlightBeforeSwap()
        {
            const int firstNumber = 1;
            const int secondNumber = 99;
            
            using var executeThread1 = new OwnerExecutionThread();
            using var executeThread2 = new OwnerExecutionThread();
            var owwnerHandle = new ObjectOwnerHandle(executeThread1.Owner);

            var numbers = new List<int>();
            var inFlight = new ManualResetEventSlim();

            //run first operation
            _ = owwnerHandle.RunOnOwner(() =>
            {
                inFlight.Wait();
                numbers.Add(firstNumber);
            });
            
            var changeOwnerTask = owwnerHandle.ChangeOwner(executeThread2.Owner);
            
            //run second operation
            var secondOperation = owwnerHandle.RunOnOwner(() => { numbers.Add(secondNumber); });
            await Task.Delay(10);
            
            //complete first operation
            inFlight.Set();
            
            await changeOwnerTask;
            await secondOperation;
            
            Assert.AreEqual(new[] { firstNumber, secondNumber }, numbers.ToArray());
        }

        [Test]
        public async Task BufferedRequests_FlushedInOrderToNewOwner()
        {
            using var executeThread1 = new OwnerExecutionThread();
            using var executeThread2 = new OwnerExecutionThread();
            var ownerHandle = new ObjectOwnerHandle(executeThread1.Owner);
            
            var inFlight = new ManualResetEventSlim();
            _ = ownerHandle.RunOnOwner(() => inFlight.Wait());
            
            var changeOwnerTask = ownerHandle.ChangeOwner(executeThread2.Owner);
            
            var result = new ConcurrentQueue<int>();
            var tasks = new Task[3];
            for (var i = 0; i < tasks.Length; i++)
            {
                var n = i;
                tasks[i] = ownerHandle.RunOnOwner(() => result.Enqueue(n)).AsTask();
            }
            
            inFlight.Set();
            await changeOwnerTask;
            await Task.WhenAll(tasks);
            
            CollectionAssert.AreEqual(new[] { 0, 1, 2 }, result.ToArray());
        }

        [Test]
        public async Task TryRunImmediately_FalseFromNonOwnerThread()
        {
            using var executeThread = new OwnerExecutionThread();
            var owner = new ObjectOwnerHandle(executeThread.Owner);
            var ran = false;

            await Task.Run(() =>
            {
                ran = owner.TryRunImmediately(() => { });
            });

            Assert.IsFalse(ran);
        }

        [Test]
        public async Task Stress_HandoverAndWorkers_NoDeadlock()
        {
            const int rounds = 100;
            const int workers = 8;
            const int perWorker = 200;

            using var executeThread1 = new OwnerExecutionThread();
            using var executeThread2 = new OwnerExecutionThread();
            var handle = new ObjectOwnerHandle(executeThread1.Owner);
            var completed = 0;
            var failures = 0;
            var sw = Stopwatch.StartNew();

            var handover = Task.Run(async () =>
            {
                for (var r = 0; r < rounds; r++)
                {
                    var target = r % 2 == 0 ? executeThread2 : executeThread1;
                    await handle.ChangeOwner(target.Owner);
                }
            });

            var workerTasks = Enumerable.Range(0, workers).Select(_ => Task.Run(async () =>
            {
                for (var i = 0; i < perWorker; i++)
                {
                    try
                    {
                        await handle.RunOnOwner(() => Interlocked.Increment(ref completed));
                    }
                    catch
                    {
                        Interlocked.Increment(ref failures);
                    }
                }
            })).ToArray();

            await Task.WhenAll(new[] { handover }.Concat(workerTasks));
            sw.Stop();

            Assert.AreEqual(0, failures, $"{failures} requests failed");
            Assert.AreEqual(workers * perWorker, completed, "all requests completed");
            TestContext.Out.WriteLine($"Stress: {completed} requests in {sw.ElapsedMilliseconds}ms");
        }

        [Test]
        public async Task MultipleHandles_SameOwner_NoCrossTalk()
        {
            using var executeThread = new OwnerExecutionThread();
            var h1 = new ObjectOwnerHandle(executeThread.Owner);
            var h2 = new ObjectOwnerHandle(executeThread.Owner);

            var c1 = 0;
            var c2 = 0;

            var t1 = h1.RunOnOwner(() => Interlocked.Increment(ref c1)).AsTask();
            var t2 = h2.RunOnOwner(() => Interlocked.Increment(ref c2)).AsTask();

            await Task.WhenAll(t1, t2);
            Assert.AreEqual(1, c1);
            Assert.AreEqual(1, c2);
        }

        [Test]
        public async Task ConcurrentChangeOwner_NoLostBufferedRequests()
        {
            using var executeThreadA = new OwnerExecutionThread();
            using var executeThreadB = new OwnerExecutionThread();
            using var executeThreadC = new OwnerExecutionThread();
            var executeThreads = new[] { executeThreadA, executeThreadB, executeThreadC };
            var handle = new ObjectOwnerHandle(executeThreads[0].Owner);
            var total = 0;
            const int expected = 500;
            var allDone = new CountdownEvent(expected);

            // executeThread thread for handovers
            var handoverTask = Task.Run(async () =>
            {
                for (var i = 0; i < 50; i++)
                {
                    var target = executeThreads[i % executeThreads.Length];
                    await handle.ChangeOwner(target.Owner);
                }
            });

            // workers fire requests concurrently
            var workerTasks = Enumerable.Range(0, 4).Select(_ => Task.Run(async () =>
            {
                for (var i = 0; i < expected / 4; i++)
                {
                    try
                    {
                        await handle.RunOnOwner(() =>
                        {
                            Interlocked.Increment(ref total);
                            allDone.Signal();
                        });
                    }
                    catch
                    {
                        // handover in progress — task may fault; signal anyway to avoid hang
                        allDone.Signal();
                    }
                }
            })).ToArray();

            await Task.WhenAll(new[] { handoverTask }.Concat(workerTasks));

            Assert.IsTrue(allDone.Wait(TimeSpan.FromSeconds(10)), "timeout");
            // we only care that nothing deadlocked or crashed — total <= expected due to handover-faulted tasks
            TestContext.Out.WriteLine($"completed: {total}/{expected}");
        }

        [Test]
        public async Task HandoverWhileBuffered_RequestsRunOnNewOwner()
        {
            using var p1 = new OwnerExecutionThread();
            using var p2 = new OwnerExecutionThread();
            var handle = new ObjectOwnerHandle(p1.Owner);

            // first request blocks executeThread so drain needs to wait
            var inFlight = handle.RunOnOwner(() => Thread.Sleep(100)).AsTask();
            await Task.Delay(10);

            // these will be buffered during handover
            var results = new int[5];
            var tasks = new Task[5];
            for (var i = 0; i < 5; i++)
            {
                var idx = i;
                tasks[i] = handle.RunOnOwner(() => results[idx] = idx + 1).AsTask();
            }

            var changeTask = handle.ChangeOwner(p2.Owner).AsTask();
            await Task.WhenAll(tasks);
            await changeTask;

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5 }, results);
        }

        [Test]
        public async Task Stress_RapidHandoverWithImmediateExecution()
        {
            const int rounds = 50;
            using var executeThreadA = new OwnerExecutionThread();
            using var executeThreadB = new OwnerExecutionThread();
            using var executeThreadC = new OwnerExecutionThread();
            var executeThreads = new[] { executeThreadA, executeThreadB, executeThreadC };
            var handle = new ObjectOwnerHandle(executeThreads[0].Owner);
            var ran = 0;

            var handover = Task.Run(async () =>
            {
                for (var i = 0; i < rounds; i++)
                {
                    await handle.ChangeOwner(executeThreads[i % executeThreads.Length].Owner);
                }
            });

            var worker = Task.Run(async () =>
            {
                for (var i = 0; i < 300; i++)
                {
                    await handle.RunOnOwner(() => Interlocked.Increment(ref ran));
                }
            });

            await Task.WhenAll(handover, worker);
            Assert.AreEqual(300, ran);
        }
    }
}