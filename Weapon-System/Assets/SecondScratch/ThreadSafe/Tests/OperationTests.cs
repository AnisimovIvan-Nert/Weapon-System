using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SecondScratch.ThreadSafe.Operations;
using SecondScratch.ThreadSafe.Operations.Common;
using SecondScratch.ThreadSafe.Tests.Mocks.Operation;

namespace SecondScratch.ThreadSafe.Tests
{
    public class OperationTests
    {
        private const int OperationCount = 5;
        private const int ThrowIndex = OperationCount - 1;
        private const int EnforceIndex = OperationCount - 3;

        [SetUp]
        public void SetUp()
        {
            if (OperationCount - ThrowIndex < 1 || ThrowIndex < 1)
                throw new InvalidOperationException("Broke assertion");
            
            if (OperationCount - EnforceIndex < 3 || EnforceIndex < 1)
                throw new InvalidOperationException("Broke assertion");
        }

        [Test]
        public async Task Operation_Execute_Test()
        {
            var context = new SimpleContext();
            var operation = SimpleOperationBuilder<SimpleContext>
                .Create((_, subjects, types, members) =>
                    new SimpleOperation(subjects, types, members))
                .Build(OperationCount, context);
            await operation.Execute();

            Assert.True(operation.IsCompleted);
            Assert.AreEqual(OperationCount, context.Counter);
            Assert.AreEqual(OperationCount * context.StatefulDiff, context.StatefulCounter);
        }

        [Test]
        public void Operation_Throw_Test()
        {
            var context = new SimpleContext();
            var operation = SimpleOperationBuilder<SimpleContext>
                .Create((i, subjects, types, members) =>
                    new SimpleOperation(subjects, types, members, i == ThrowIndex))
                .Build(OperationCount, context);

            Assert.CatchAsync<SimpleOperationException>(async () => await operation.Execute());

            Assert.True(operation.IsCompleted);
            Assert.AreEqual(0, context.Counter);
            Assert.AreEqual(0, context.StatefulCounter);
        }

        [Test]
        public async Task Middleware_BeforeExecution_Test()
        {
            var context = new SimpleContext();
            var operation = SimpleOperationBuilder<SimpleContext>
                .Create((_, subjects, types, members) =>
                    new SimpleOperation(subjects, types, members))
                .WithMiddlewares(new List<IOperationMiddleware>
                {
                    new SimpleMiddleware<SimpleOperation, SimpleContext>
                    {
                        BeforeExecutionAction = (_, _, c) => c.Counter *= 2
                    }
                })
                .Build(OperationCount, context);

            await operation.Execute();

            var exceptedContext = new SimpleContext();
            for (var i = 0; i < OperationCount; i++)
            {
                exceptedContext.Counter *= 2;
                await exceptedContext.Mutate();
            }

            Assert.True(operation.IsCompleted);
            Assert.AreEqual(exceptedContext.Counter, context.Counter);
            Assert.AreEqual(OperationCount * context.StatefulDiff, context.StatefulCounter);
        }

        [Test]
        public async Task Middleware_AfterExecution_Test()
        {
            var context = new SimpleContext();
            var operation = SimpleOperationBuilder<SimpleContext>
                .Create((_, subjects, types, members) =>
                    new SimpleOperation(subjects, types, members))
                .WithMiddlewares(new List<IOperationMiddleware>
                {
                    new SimpleMiddleware<SimpleOperation, SimpleContext>
                    {
                        AfterExecutionAction = (_, _, c) => c.Counter *= 2
                    }
                })
                .Build(OperationCount, context);

            await operation.Execute();

            var exceptedContext = new SimpleContext();
            for (var i = 0; i < OperationCount; i++)
            {
                await exceptedContext.Mutate();
                exceptedContext.Counter *= 2;
            }

            Assert.True(operation.IsCompleted);
            Assert.AreEqual(exceptedContext.Counter, context.Counter);
            Assert.AreEqual(OperationCount * context.StatefulDiff, context.StatefulCounter);
        }

        [Test]
        public async Task Middleware_TryHandleException_Test()
        {
            var context = new SimpleContext();
            var operation = SimpleOperationBuilder<SimpleContext>
                .Create((i, subjects, types, members) =>
                    new SimpleOperation(subjects, types, members, i == ThrowIndex))
                .WithMiddlewares(new List<IOperationMiddleware>
                {
                    new SimpleMiddleware<SimpleOperation, SimpleContext>
                    {
                        TryHandleExceptionAction = (_, _, c) =>
                        {
                            c.Counter += 1;
                            return true;
                        }
                    }
                })
                .Build(OperationCount, context);

            await operation.Execute();

            Assert.True(operation.IsCompleted);
            Assert.AreEqual(OperationCount + 1, context.Counter);
            Assert.AreEqual(OperationCount * context.StatefulDiff, context.StatefulCounter);
        }

        [Test]
        public void Middleware_BeforeExecutionThrow_Test()
        {
            var context = new SimpleContext();
            var operation = SimpleOperationBuilder<SimpleContext>
                .Create((_, subjects, types, members) =>
                    new SimpleOperation(subjects, types, members))
                .WithMiddlewares(new List<IOperationMiddleware>
                {
                    new SimpleMiddleware<SimpleOperation, SimpleContext>
                    {
                        BeforeExecutionAction = (_, _, _) => throw new TestException()
                    }
                })
                .Build(OperationCount, context);

            Assert.CatchAsync<TestException>(async () => await operation.Execute());

            Assert.True(operation.IsCompleted);
            Assert.AreEqual(0, context.Counter);
            Assert.AreEqual(0, context.StatefulCounter);
        }

        [Test]
        public void Middleware_AfterExecutionThrow_Test()
        {
            var context = new SimpleContext();
            var operation = SimpleOperationBuilder<SimpleContext>
                .Create((_, subjects, types, members) =>
                    new SimpleOperation(subjects, types, members))
                .WithMiddlewares(new List<IOperationMiddleware>
                {
                    new SimpleMiddleware<SimpleOperation, SimpleContext>
                    {
                        AfterExecutionAction = (_, _, _) => throw new TestException()
                    }
                })
                .Build(OperationCount, context);

            Assert.CatchAsync<TestException>(async () => await operation.Execute());

            Assert.True(operation.IsCompleted);
            Assert.AreEqual(0, context.Counter);
            Assert.AreEqual(0, context.StatefulCounter);
        }

        [Test]
        public void Middleware_TryHandleExceptionThrow_Test()
        {
            var context = new SimpleContext();
            var operation = SimpleOperationBuilder<SimpleContext>
                .Create((i, subjects, types, members) =>
                    new SimpleOperation(subjects, types, members, i == ThrowIndex))
                .WithMiddlewares(new List<IOperationMiddleware>
                {
                    new SimpleMiddleware<SimpleOperation, SimpleContext>
                    {
                        TryHandleExceptionAction = (_, _, _) => throw new TestException()
                    }
                })
                .Build(OperationCount, context);

            Assert.CatchAsync<TestException>(async () => await operation.Execute());

            Assert.True(operation.IsCompleted);
            Assert.AreEqual(0, context.Counter);
            Assert.AreEqual(0, context.StatefulCounter);
        }

        [Test]
        public void Subject_Upstream_Test()
        {
            var context = new SimpleContext();
            var operation = SimpleOperationBuilder<SimpleContext>
                .Create((_, subjects, types, members) =>
                    new SimpleOperation(subjects, types, members))
                .WithSubjects(i => i != ThrowIndex
                    ? null
                    : new List<IOperationSubject>
                    {
                        new SimpleOperationSubject<SimpleOperation, SimpleContext>
                        {
                            UpstreamOperationAction = (_, _, _) => throw new TestException()
                        }
                    })
                .Build(OperationCount, context);

            Assert.CatchAsync<TestException>(async () => await operation.Execute());

            Assert.True(operation.IsCompleted);
            Assert.AreEqual(0, context.Counter);
            Assert.AreEqual(0, context.StatefulCounter);
        }
        
        [Test]
        public void Subject_Downstream_Test()
        {
            var context = new SimpleContext();
            var operation = SimpleOperationBuilder<SimpleContext>
                .Create((_, subjects, types, members) =>
                    new SimpleOperation(subjects, types, members))
                .WithSubjects(i => i != ThrowIndex
                    ? null
                    : new List<IOperationSubject>
                    {
                        new SimpleOperationSubject<SimpleOperation, SimpleContext>
                        {
                            DownstreamOperationAction = (_, _, _) => throw new TestException()
                        }
                    })
                .Build(OperationCount, context);

            Assert.CatchAsync<TestException>(async () => await operation.Execute());

            Assert.True(operation.IsCompleted);
            Assert.AreEqual(0, context.Counter);
            Assert.AreEqual(0, context.StatefulCounter);
        }

        [Test]
        public async Task Operation_EnforceCompletion_Cascade_Test()
        {
            const int exceptedOperationCount = EnforceIndex;
            
            var context = new SimpleContext();
            var operation = SimpleOperationBuilder<SimpleContext>
                .Create((i, subjects, types, members) =>
                    new SimpleOperation(subjects, types, members, false, i == EnforceIndex, true))
                .Build(OperationCount, context);

            await operation.Execute();
            
            Assert.True(operation.IsCompleted);
            Assert.AreEqual(exceptedOperationCount, context.Counter);
            Assert.AreEqual(exceptedOperationCount * context.StatefulDiff, context.StatefulCounter);
        }
        
        [Test]
        public async Task Operation_EnforceCompletion_Test()
        {
            const int exceptedOperationCount = OperationCount - 1;
            
            var context = new SimpleContext();
            var operation = SimpleOperationBuilder<SimpleContext>
                .Create((i, subjects, types, members) =>
                    new SimpleOperation(subjects, types, members, false, i == EnforceIndex))
                .Build(OperationCount, context);

            await operation.Execute();
            
            Assert.True(operation.IsCompleted);
            Assert.AreEqual(exceptedOperationCount, context.Counter);
            Assert.AreEqual(exceptedOperationCount * context.StatefulDiff, context.StatefulCounter);
        }
        
        [Test]
        public async Task Middleware_BeforeExecution_EnforceCompletion_Cascade_Test()
        {
            const int exceptedOperationCount = EnforceIndex;
            
            var context = new SimpleContext();
            var operation = SimpleOperationBuilder<SimpleContext>
                .Create((_, subjects, types, members) =>
                    new SimpleOperation(subjects, types, members))
                .WithMembers(i => i != EnforceIndex 
                    ? null 
                    : new List<OperationMember>
                    {
                        new(Members.Executor, null!)
                    })
                .WithMiddlewares(new List<IOperationMiddleware>
                {
                    new SimpleMiddleware<SimpleOperation, SimpleContext>
                    {
                        BeforeExecutionAction = (o, _, _) =>
                        {
                            if (o.Members.Any(m => m.Type == Members.Executor))
                                throw new OperationEnforceComplete(true);
                        }
                    }
                })
                .Build(OperationCount, context);

            await operation.Execute();
            
            Assert.True(operation.IsCompleted);
            Assert.AreEqual(exceptedOperationCount, context.Counter);
            Assert.AreEqual(exceptedOperationCount * context.StatefulDiff, context.StatefulCounter);
        }
        
        [Test]
        public async Task Middleware_BeforeExecution_EnforceCompletion_Test()
        {
            const int exceptedOperationCount = OperationCount - 1;
            
            var context = new SimpleContext();
            var operation = SimpleOperationBuilder<SimpleContext>
                .Create((_, subjects, types, members) =>
                    new SimpleOperation(subjects, types, members))
                .WithMembers(i => i != EnforceIndex 
                    ? null 
                    : new List<OperationMember>
                    {
                        new(Members.Executor, null!)
                    })
                .WithMiddlewares(new List<IOperationMiddleware>
                {
                    new SimpleMiddleware<SimpleOperation, SimpleContext>
                    {
                        BeforeExecutionAction = (o, _, _) =>
                        {
                            if (o.Members.Any(m => m.Type == Members.Executor))
                                throw new OperationEnforceComplete(false);
                        }
                    }
                })
                .Build(OperationCount, context);

            await operation.Execute();
            
            Assert.True(operation.IsCompleted);
            Assert.AreEqual(exceptedOperationCount, context.Counter);
            Assert.AreEqual(exceptedOperationCount * context.StatefulDiff, context.StatefulCounter);
        }
        
        [Test]
        public async Task Middleware_AfterExecution_EnforceCompletion_Cascade_Test()
        {
            const int exceptedOperationCount = EnforceIndex + 1;
            
            var context = new SimpleContext();
            var operation = SimpleOperationBuilder<SimpleContext>
                .Create((_, subjects, types, members) =>
                    new SimpleOperation(subjects, types, members))
                .WithMembers(i => i != EnforceIndex 
                    ? null 
                    : new List<OperationMember>
                    {
                        new(Members.Executor, null!)
                    })
                .WithMiddlewares(new List<IOperationMiddleware>
                {
                    new SimpleMiddleware<SimpleOperation, SimpleContext>
                    {
                        AfterExecutionAction = (o, _, _) =>
                        {
                            if (o.Members.Any(m => m.Type == Members.Executor))
                                throw new OperationEnforceComplete(true);
                        }
                    }
                })
                .Build(OperationCount, context);

            await operation.Execute();
            
            Assert.True(operation.IsCompleted);
            Assert.AreEqual(exceptedOperationCount, context.Counter);
            Assert.AreEqual(exceptedOperationCount * context.StatefulDiff, context.StatefulCounter);
        }
        
        [Test]
        public async Task Middleware_AfterExecution_EnforceCompletion_Test()
        {
            const int exceptedOperationCount = OperationCount;
            
            var context = new SimpleContext();
            var operation = SimpleOperationBuilder<SimpleContext>
                .Create((_, subjects, types, members) =>
                    new SimpleOperation(subjects, types, members))
                .WithMembers(i => i != EnforceIndex 
                    ? null 
                    : new List<OperationMember>
                    {
                        new(Members.Executor, null!)
                    })
                .WithMiddlewares(new List<IOperationMiddleware>
                {
                    new SimpleMiddleware<SimpleOperation, SimpleContext>
                    {
                        AfterExecutionAction = (o, _, _) =>
                        {
                            if (o.Members.Any(m => m.Type == Members.Executor))
                                throw new OperationEnforceComplete(false);
                        }
                    }
                })
                .Build(OperationCount, context);

            await operation.Execute();
            
            Assert.True(operation.IsCompleted);
            Assert.AreEqual(exceptedOperationCount, context.Counter);
            Assert.AreEqual(exceptedOperationCount * context.StatefulDiff, context.StatefulCounter);
        }
        
        [Test]
        public async Task Middleware_TryHandleException_EnforceCompletion_Cascade_Test()
        {
            const int exceptedOperationCount = EnforceIndex + 1;
            
            var context = new SimpleContext();
            var operation = SimpleOperationBuilder<SimpleContext>
                .Create((i, subjects, types, members) =>
                    new SimpleOperation(subjects, types, members, i == EnforceIndex))
                .WithMiddlewares(new List<IOperationMiddleware>
                {
                    new SimpleMiddleware<SimpleOperation, SimpleContext>
                    {
                        TryHandleExceptionAction = (_, _, _) => throw new OperationEnforceComplete(true)
                    }
                })
                .Build(OperationCount, context);

            await operation.Execute();
            
            Assert.True(operation.IsCompleted);
            Assert.AreEqual(exceptedOperationCount, context.Counter);
            Assert.AreEqual(exceptedOperationCount * context.StatefulDiff, context.StatefulCounter);
        }
        
        [Test]
        public async Task Middleware_TryHandleException_EnforceCompletion_Test()
        {
            const int exceptedOperationCount = OperationCount;
            
            var context = new SimpleContext();
            var operation = SimpleOperationBuilder<SimpleContext>
                .Create((i, subjects, types, members) =>
                    new SimpleOperation(subjects, types, members, i == EnforceIndex))
                .WithMiddlewares(new List<IOperationMiddleware>
                {
                    new SimpleMiddleware<SimpleOperation, SimpleContext>
                    {
                        TryHandleExceptionAction = (_, _, _) => throw new OperationEnforceComplete(false)
                    }
                })
                .Build(OperationCount, context);

            await operation.Execute();
            
            Assert.True(operation.IsCompleted);
            Assert.AreEqual(exceptedOperationCount, context.Counter);
            Assert.AreEqual(exceptedOperationCount * context.StatefulDiff, context.StatefulCounter);
        }
        
        [Test]
        public async Task Subject_Upstream_EnforceCompletion_Cascade_Test()
        {
            const int exceptedOperationCount = EnforceIndex;
            
            var context = new SimpleContext();
            var operation = SimpleOperationBuilder<SimpleContext>
                .Create((_, subjects, types, members) =>
                    new SimpleOperation(subjects, types, members))
                .WithSubjects(i => i != EnforceIndex
                    ? null
                    : new List<IOperationSubject>
                    {
                        new SimpleOperationSubject<SimpleOperation, SimpleContext>
                        {
                            UpstreamOperationAction = (_, _, _) => throw new OperationEnforceComplete(true)
                        }
                    })
                .Build(OperationCount, context);

            await operation.Execute();
            
            Assert.True(operation.IsCompleted);
            Assert.AreEqual(exceptedOperationCount, context.Counter);
            Assert.AreEqual(exceptedOperationCount * context.StatefulDiff, context.StatefulCounter);
        }
        
        [Test]
        public async Task Subject_Upstream_EnforceCompletion_Test()
        {
            const int exceptedOperationCount = OperationCount - 1;
            
            var context = new SimpleContext();
            var operation = SimpleOperationBuilder<SimpleContext>
                .Create((_, subjects, types, members) =>
                    new SimpleOperation(subjects, types, members))
                .WithSubjects(i => i != EnforceIndex
                    ? null
                    : new List<IOperationSubject>
                    {
                        new SimpleOperationSubject<SimpleOperation, SimpleContext>
                        {
                            UpstreamOperationAction = (_, _, _) => throw new OperationEnforceComplete(false)
                        }
                    })
                .Build(OperationCount, context);

            await operation.Execute();
            
            Assert.True(operation.IsCompleted);
            Assert.AreEqual(exceptedOperationCount, context.Counter);
            Assert.AreEqual(exceptedOperationCount * context.StatefulDiff, context.StatefulCounter);
        }
        
        [Test]
        public async Task Subject_Downstream_EnforceCompletion_Cascade_Test()
        {
            const int exceptedOperationCount = EnforceIndex;
            
            var context = new SimpleContext();
            var operation = SimpleOperationBuilder<SimpleContext>
                .Create((_, subjects, types, members) =>
                    new SimpleOperation(subjects, types, members))
                .WithSubjects(i => i != EnforceIndex
                    ? null
                    : new List<IOperationSubject>
                    {
                        new SimpleOperationSubject<SimpleOperation, SimpleContext>
                        {
                            DownstreamOperationAction = (_, _, _) => throw new OperationEnforceComplete(true)
                        }
                    })
                .Build(OperationCount, context);

            await operation.Execute();
            
            Assert.True(operation.IsCompleted);
            Assert.AreEqual(exceptedOperationCount, context.Counter);
            Assert.AreEqual(exceptedOperationCount * context.StatefulDiff, context.StatefulCounter);
        }
        
        [Test]
        public async Task Subject_Downstream_EnforceCompletion_Test()
        {
            const int exceptedOperationCount = OperationCount - 1;
            
            var context = new SimpleContext();
            var operation = SimpleOperationBuilder<SimpleContext>
                .Create((_, subjects, types, members) =>
                    new SimpleOperation(subjects, types, members))
                .WithSubjects(i => i != EnforceIndex
                    ? null
                    : new List<IOperationSubject>
                    {
                        new SimpleOperationSubject<SimpleOperation, SimpleContext>
                        {
                            DownstreamOperationAction = (_, _, _) => throw new OperationEnforceComplete(false)
                        }
                    })
                .Build(OperationCount, context);

            await operation.Execute();
            
            Assert.True(operation.IsCompleted);
            Assert.AreEqual(exceptedOperationCount, context.Counter);
            Assert.AreEqual(exceptedOperationCount * context.StatefulDiff, context.StatefulCounter);
        }

        private class TestException : Exception
        {
        }

        public class SimpleOperationBuilder<TContext>
            where TContext : IOperationContext
        {
            public delegate IOperation<TContext> OperationFactory(int i,
                List<IOperationSubject> subjects, List<OperationTypes> types, List<OperationMember> members);

            private Func<int, List<IOperationSubject>?>? _subjectFactory;
            private Func<int, List<OperationTypes>?>? _typesFactory;
            private Func<int, List<OperationMember>?>? _memberFactory;

            private IEnumerable<IOperationMiddleware>? _middlewares;

            private readonly OperationFactory _operationFactory;

            private SimpleOperationBuilder(OperationFactory operationFactory)
            {
                _operationFactory = operationFactory;
            }

            public static SimpleOperationBuilder<TContext> Create(OperationFactory factory) => new(factory);

            public SimpleOperationBuilder<TContext> WithSubjects(Func<int, List<IOperationSubject>?> factory)
            {
                _subjectFactory = factory;
                return this;
            }

            public SimpleOperationBuilder<TContext> WithTypes(Func<int, List<OperationTypes>?> factory)
            {
                _typesFactory = factory;
                return this;
            }

            public SimpleOperationBuilder<TContext> WithMembers(Func<int, List<OperationMember>?> factory)
            {
                _memberFactory = factory;
                return this;
            }

            public SimpleOperationBuilder<TContext> WithMiddlewares(IEnumerable<IOperationMiddleware> middlewares)
            {
                _middlewares = middlewares;
                return this;
            }


            public OperationHandler<TContext> Build(int operationCount, TContext context)
            {
                _middlewares ??= new List<IOperationMiddleware>();

                var operations = new IOperation<TContext>[operationCount];
                for (var i = 0; i < operationCount; i++)
                {
                    var subject = _subjectFactory?.Invoke(i) ?? new List<IOperationSubject>();
                    var types = _typesFactory?.Invoke(i) ?? new List<OperationTypes>();
                    var members = _memberFactory?.Invoke(i) ?? new List<OperationMember>();

                    operations[i] = _operationFactory(i, subject, types, members);
                }

                var middlewareSource = new SimpleOperationMiddlewareSource(_middlewares.ToArray());

                return new OperationHandler<TContext>(middlewareSource, context, operations);
            }
        }
    }
}