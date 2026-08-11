using System.Collections;
using OperationSystem.Operations;
using OperationSystem.Operations.Abstract;
using OperationSystem.Operations.Data;
using OperationSystem.Operations.Middleware;
using OperationSystem.Operations.Result;
using Unity.Collections;
using UnityEngine;

namespace OperationSystem.ComplexWeapons.Operations
{
    public class RaycastOperation : AbstractOperation
    {
        private NativeArray<RaycastCommand> _commands;
        private NativeArray<RaycastHit> _results;
        
        public RaycastOperation(Data operationData, OperationIdentifier identifier, IOperationMiddleware[] middlewares)
            : base(identifier, middlewares, operationData)
        {
            _commands = new NativeArray<RaycastCommand>(1, Allocator.Persistent);
            _results = new NativeArray<RaycastHit>(1, Allocator.Persistent);
        }
        
        protected override IEnumerator ExecuteEnumerator()
        {
            yield return base.ExecuteEnumerator();

            var data = this.GetData<Data>();
            _commands[0] = data.Command;

            var handle = RaycastCommand.ScheduleBatch(_commands, _results, 1);
            while (!handle.IsCompleted)
                yield return null;

            OperationResult = new Result(_results[0]);
        }

        protected override void Dispose()
        {
            base.Dispose();

            _commands.Dispose();
            _results.Dispose();
        }

        public readonly struct Data : IOperationData
        {
            public RaycastCommand Command { get; }
            
            public Data(RaycastCommand command)
            {
                Command = command;
            }
        }

        public readonly struct Result : IOperationResult
        {
            public RaycastHit Hit { get; }
            
            public Result(RaycastHit hit)
            {
                Hit = hit;
            }
        }
    }
}