using System.Collections;
using OperationSystem.Operations;
using OperationSystem.Operations.Abstract;
using OperationSystem.Operations.Data;
using OperationSystem.Operations.Result;
using OperationSystem.Operations.Tags;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace OperationSystem.ComplexWeapons.Operations.Hit
{
    public interface IRaycastOperationTag : IOperationTag
    {
    }
    
    public class RaycastOperation : AbstractOperation, IRaycastOperationTag
    {
        private NativeArray<RaycastCommand> _commands;
        private NativeArray<RaycastHit> _results;
        private JobHandle? _handle;
        
        public RaycastOperation(OperationIdentifier identifier, Data data)
            : base(identifier, data)
        {
            _commands = new NativeArray<RaycastCommand>(1, Allocator.Persistent);
            _results = new NativeArray<RaycastHit>(1, Allocator.Persistent);
        }
        
        protected override IEnumerator ExecuteEnumerator()
        {
            yield return base.ExecuteEnumerator();

            var data = this.GetData<Data>();
            _commands[0] = data.Command;

            _handle = RaycastCommand.ScheduleBatch(_commands, _results, 1);
            while (!_handle.Value.IsCompleted)
                yield return null;
            
            _handle.Value.Complete();
            SetResult(new Result(_results[0]));
        }

        protected override void Dispose()
        {
            base.Dispose();

            if (_handle != null)
            {
                _commands.Dispose(_handle.Value);
                _results.Dispose(_handle.Value);
            }
            else
            {
                _commands.Dispose();
                _results.Dispose();
            }
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