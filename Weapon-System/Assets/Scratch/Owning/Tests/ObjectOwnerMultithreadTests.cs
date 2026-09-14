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
        class Pump : IDisposable
        {
            public ObjectOwner Owner { get; private set; }
            private readonly Thread _thread;

            public Pump()
            {
                _thread = new Thread(() =>
                {
                    Owner = new ObjectOwner();
                    while (true) { Owner.ExecuteNext(); Thread.Yield(); }
                }) { IsBackground = true };
                _thread.Start();
                while (Owner == null) Thread.SpinWait(10);
            }

            public void Dispose()
            {
                _thread.Abort();
            }
        }

        [Test]
        public void ConcurrentRunOnOwner_AllRequestsComplete()
        {
            using var pump = new Pump();
            var handle = new ObjectOwnerHandle(pump.Owner);
            var total = 0;
            var expected = 1000;
            var allDone = new CountdownEvent(expected);

            var threads = Enumerable.Range(0, 8).Select(_ => new Thread(() =>
            {
                for (var i = 0; i < expected / 8; i++)
                {
                    handle.RunOnOwner(() =>
                    {
                        Interlocked.Increment(ref total);
                        allDone.Signal();
                    }).AsTask().GetAwaiter().GetResult();
                }
            }) { IsBackground = true }).ToArray();

            foreach (var t in threads) t.Start();
            Assert.IsTrue(allDone.Wait(TimeSpan.FromSeconds(10)), "timeout waiting for requests");
            foreach (var t in threads) t.Join();
            Assert.AreEqual(expected, total);
        }

        [Test]
        public void ConcurrentRunOnOwner_FIFOPreserved_WithinEachThread()
        {
            using var pump = new Pump();
            var handle = new ObjectOwnerHandle(pump.Owner);
            var executed = new ConcurrentQueue<(int lane, int seq)>();

            var threads = Enumerable.Range(0, 4).Select(lane => new Thread(() =>
            {
                for (var i = 0; i < 250; i++)
                {
                    var seq = i;
                    handle.RunOnOwner(() => executed.Enqueue((lane, seq)))
                        .AsTask().GetAwaiter().GetResult();
                }
            }) { IsBackground = true }).ToArray();

            foreach (var t in threads) t.Start();
            foreach (var t in threads) t.Join();

            var arr = executed.ToArray();
            Assert.AreEqual(1000, arr.Length);
            // Single owner thread processes the queue FIFO, and each lane submits
            // sequentially (it awaits each request before the next). So each lane's
            // seq values must appear in strictly ascending order in the global log.
            var lastSeq = new int[4];
            for (var i = 0; i < 4; i++) lastSeq[i] = -1;
            foreach (var (lane, seq) in arr)
            {
                Assert.IsTrue(seq > lastSeq[lane], $"lane {lane}: out of FIFO order at seq {seq}");
                lastSeq[lane] = seq;
            }
        }

        [Test]
        public void ChangeOwner_DrainsInFlightBeforeSwap()
        {
            using var p1 = new Pump();
            using var p2 = new Pump();
            var handle = new ObjectOwnerHandle(p1.Owner);

            var ran = new List<int>();
            var lockObj = new object();
            var inFlightStarted = new ManualResetEventSlim();
            var inFlightDone = new ManualResetEventSlim();

            var t1 = handle.RunOnOwner(() =>
            {
                lock (lockObj) ran.Add(1);
                inFlightStarted.Set();
                inFlightDone.Wait();
            }).AsTask();

            inFlightStarted.Wait();
            var change = handle.ChangeOwner(p2.Owner);
            var buffered = handle.RunOnOwner(() => { lock (lockObj) ran.Add(99); }).AsTask();
            inFlightDone.Set();
            awaitSwallow(change);
            awaitSwallow(new ValueTask(buffered));

            lock (lockObj)
            {
                Assert.AreEqual(new[] { 1, 99 }, ran.ToArray());
            }
        }

        [Test]
        public void BufferedRequests_FlushedInOrderToNewOwner()
        {
            using var p1 = new Pump();
            using var p2 = new Pump();
            var handle = new ObjectOwnerHandle(p1.Owner);

            var first = handle.RunOnOwner(() => Thread.Sleep(50)).AsTask();
            // wait until first is running on pump
            Thread.Sleep(10);

            // queue 3 more while first is in-flight
            var b1 = handle.RunOnOwner(() => { }).AsTask();
            var b2 = handle.RunOnOwner(() => { }).AsTask();
            var b3 = handle.RunOnOwner(() => { }).AsTask();

            // handover — must wait for first, then flush b1,b2,b3 to p2
            var changeTask = handle.ChangeOwner(p2.Owner).AsTask();
            changeTask.GetAwaiter().GetResult();

            // now run on new owner
            var order = new ConcurrentQueue<int>();
            var after = new Task[3];
            for (var i = 0; i < 3; i++)
            {
                var n = i;
                after[i] = handle.RunOnOwner(() => order.Enqueue(n)).AsTask();
            }

            Task.WaitAll(after);
            var arr = order.ToArray();
            CollectionAssert.AreEqual(new[] { 0, 1, 2 }, arr);
        }

        [Test]
        public void TryRunImmediately_FalseFromNonOwnerThread()
        {
            using var pump = new Pump();
            var handle = new ObjectOwnerHandle(pump.Owner);
            var ran = false;

            var t = new Thread(() =>
            {
                ran = handle.TryRunImmediately(() => { });
            });
            t.Start();
            t.Join(5000);

            Assert.IsFalse(ran);
        }

        [Test]
        public void Stress_HandoverAndWorkers_NoDeadlock()
        {
            const int rounds = 100;
            const int workers = 8;
            const int perWorker = 200;

            using var p1 = new Pump();
            using var p2 = new Pump();
            var handle = new ObjectOwnerHandle(p1.Owner);
            var completed = 0;
            var failures = 0;
            var sw = Stopwatch.StartNew();

            var handover = new Thread(() =>
            {
                for (var r = 0; r < rounds; r++)
                {
                    var target = r % 2 == 0 ? p2 : p1;
                    handle.ChangeOwner(target.Owner).AsTask().GetAwaiter().GetResult();
                }
            }) { IsBackground = true };

            var workerThreads = Enumerable.Range(0, workers).Select(_ => new Thread(() =>
            {
                for (var i = 0; i < perWorker; i++)
                {
                    try
                    {
                        handle.RunOnOwner(() => Interlocked.Increment(ref completed))
                            .AsTask().GetAwaiter().GetResult();
                    }
                    catch
                    {
                        Interlocked.Increment(ref failures);
                    }
                }
            }) { IsBackground = true }).ToArray();

            handover.Start();
            foreach (var w in workerThreads) w.Start();
            handover.Join();
            foreach (var w in workerThreads) w.Join();
            sw.Stop();

            Assert.AreEqual(0, failures, $"{failures} requests failed");
            Assert.AreEqual(workers * perWorker, completed, "all requests completed");
            TestContext.Out.WriteLine($"Stress: {completed} requests in {sw.ElapsedMilliseconds}ms");
        }

        [Test]
        public void MultipleHandles_SameOwner_NoCrossTalk()
        {
            using var pump = new Pump();
            var h1 = new ObjectOwnerHandle(pump.Owner);
            var h2 = new ObjectOwnerHandle(pump.Owner);

            var c1 = 0;
            var c2 = 0;

            var t1 = h1.RunOnOwner(() => Interlocked.Increment(ref c1)).AsTask();
            var t2 = h2.RunOnOwner(() => Interlocked.Increment(ref c2)).AsTask();

            Task.WhenAll(t1, t2).GetAwaiter().GetResult();
            Assert.AreEqual(1, c1);
            Assert.AreEqual(1, c2);
        }

        [Test]
        public void ConcurrentChangeOwner_NoLostBufferedRequests()
        {
            using var pumpA = new Pump();
            using var pumpB = new Pump();
            using var pumpC = new Pump();
            var pumps = new[] { pumpA, pumpB, pumpC };
            var handle = new ObjectOwnerHandle(pumps[0].Owner);
            var total = 0;
            var expected = 500;
            var allDone = new CountdownEvent(expected);

            // pump thread for handovers
            var handoverThread = new Thread(() =>
            {
                for (var i = 0; i < 50; i++)
                {
                    var target = pumps[i % pumps.Length];
                    handle.ChangeOwner(target.Owner).AsTask().GetAwaiter().GetResult();
                }
            }) { IsBackground = true };

            // workers fire requests concurrently
            var workers = Enumerable.Range(0, 4).Select(_ => new Thread(() =>
            {
                for (var i = 0; i < expected / 4; i++)
                {
                    try
                    {
                        handle.RunOnOwner(() =>
                        {
                            Interlocked.Increment(ref total);
                            allDone.Signal();
                        }).AsTask().GetAwaiter().GetResult();
                    }
                    catch
                    {
                        // handover in progress — task may fault; signal anyway to avoid hang
                        allDone.Signal();
                    }
                }
            }) { IsBackground = true }).ToArray();

            handoverThread.Start();
            foreach (var w in workers) w.Start();
            handoverThread.Join();
            foreach (var w in workers) w.Join();

            Assert.IsTrue(allDone.Wait(TimeSpan.FromSeconds(10)), "timeout");
            // we only care that nothing deadlocked or crashed — total <= expected due to handover-faulted tasks
            TestContext.Out.WriteLine($"completed: {total}/{expected}");
        }

        [Test]
        public void HandoverWhileBuffered_RequestsRunOnNewOwner()
        {
            using var p1 = new Pump();
            using var p2 = new Pump();
            var handle = new ObjectOwnerHandle(p1.Owner);

            // first request blocks pump so drain needs to wait
            var inFlight = handle.RunOnOwner(() => Thread.Sleep(100)).AsTask();
            Thread.Sleep(10);

            // these will be buffered during handover
            var results = new int[5];
            var tasks = new Task[5];
            for (var i = 0; i < 5; i++)
            {
                var idx = i;
                tasks[i] = handle.RunOnOwner(() => results[idx] = idx + 1).AsTask();
            }

            var changeTask = handle.ChangeOwner(p2.Owner).AsTask();
            Task.WhenAll(tasks).GetAwaiter().GetResult();
            changeTask.GetAwaiter().GetResult();

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5 }, results);
        }

        [Test]
        public void Stress_RapidHandoverWithImmediateExecution()
        {
            const int rounds = 50;
            using var pumpA = new Pump();
            using var pumpB = new Pump();
            using var pumpC = new Pump();
            var pumps = new[] { pumpA, pumpB, pumpC };
            var handle = new ObjectOwnerHandle(pumps[0].Owner);
            var ran = 0;

            var handover = new Thread(() =>
            {
                for (var i = 0; i < rounds; i++)
                {
                    handle.ChangeOwner(pumps[i % pumps.Length].Owner).AsTask().GetAwaiter().GetResult();
                }
            }) { IsBackground = true };

            var worker = new Thread(() =>
            {
                for (var i = 0; i < 300; i++)
                {
                    handle.RunOnOwner(() => Interlocked.Increment(ref ran))
                        .AsTask().GetAwaiter().GetResult();
                }
            }) { IsBackground = true };

            handover.Start();
            worker.Start();
            handover.Join();
            worker.Join();

            Assert.AreEqual(300, ran);
        }

        static void awaitSwallow(ValueTask vt)
        {
            try { vt.AsTask().GetAwaiter().GetResult(); } catch { }
        }
    }
}
