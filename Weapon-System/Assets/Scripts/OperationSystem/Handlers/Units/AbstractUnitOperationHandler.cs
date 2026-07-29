using System.Collections;
using OperationSystem.Assets;
using OperationSystem.Operations;
using OperationSystem.Units;

namespace OperationSystem.Handlers.Units
{
    public abstract class AbstractUnitOperationHandler
        : AbstractOperationHandler
        , IUnitOperationHandler
    {
        private readonly UnitWorld _world;
        
        public Unit? Unit { get; private set; }

        protected AbstractUnitOperationHandler(IOperationRunner operationRunner) 
            : base(operationRunner)
        {
            _world = new UnitWorld();
        }

        public IEnumerator SetUnit(IAsset? asset)
        {
            var delayer = OperationRunner.DelayOperationRunning();
            {
                if (OperationRunner.AnyRunningOperation)
                    yield return null;
                
                if (asset == null)
                    Unit = null;
                else
                    Unit = _world.CreateUnit(asset);
            }
            OperationRunner.ReleaseOperationRunning(delayer);
        }

        public override void Update()
        {
            Unit?.ComponentsData.PullData();
            base.Update();
            Unit?.ComponentsData.PushData();
        }
    }
}