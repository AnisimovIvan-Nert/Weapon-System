using System.Collections;
using OperationSystem.ComplexWeapons.Operations.Hit.Result;
using OperationSystem.Operations;
using OperationSystem.Operations.Abstract;
using OperationSystem.Operations.Data;
using UnityEngine;

namespace OperationSystem.ComplexWeapons.Operations.Shot.Stages
{
    public class BulletFlightOperation : AbstractOperation
    {
        public BulletFlightOperation(OperationIdentifier identifier, Data data, IOperationExecutor executor)
            : base(identifier, data, executor)
        {
        }

        protected override IEnumerator ExecuteEnumerator()
        {
            yield return base.ExecuteEnumerator();
            
            var data = this.GetData<Data>();
            var executor = this.GetData<IOperationExecutor>();

            yield return HitSearch(data, executor);
        }

        private IEnumerator HitSearch(Data data, IOperationExecutor executor)
        {
            var hitSearchData = new BulletHitSearchOperation.Data(data.Command);
            var hitSearchOperation = new BulletHitSearchOperation(Identifier, hitSearchData, executor);
            RunOperation(hitSearchOperation);
            yield return hitSearchOperation.WaitEnumerator();
            
            var result = hitSearchOperation.GetResult<IHitResult>();
            switch (result)
            {
                case RedirectHitResult redirect:
                    var newData = new Data(redirect.Command);
                    yield return HitSearch(newData, executor);
                    break;
                default:
                    SetResult(result);
                    break;
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
    }
}