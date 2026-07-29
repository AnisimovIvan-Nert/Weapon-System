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
        
        private IAsset? _asset;
        
        public Unit? Unit { get; private set; }

        protected AbstractUnitOperationHandler(IOperationRunner operationRunner) 
            : base(operationRunner)
        {
            _world = new UnitWorld();
        }

        public IEnumerator SetAsset(IAsset? asset)
        {
            var delayer = OperationRunner.DelayOperationRunning();
            {
                if (OperationRunner.AnyRunningOperation)
                    yield return null;

                _asset = asset;
                
                _world.Clear();
                if (asset == null)
                {
                    Unit = null;
                }
                else
                {
                    var unit = _world.GetOrAddUnit(asset);
                    Unit = unit;
                }
            }
            OperationRunner.ReleaseOperationRunning(delayer);
        }

        public override void Update()
        {
            if (_asset != null && Unit != null)
                _asset.BeforeUpdate(Unit.Value, _world);
                
            base.Update();
            
            if (_asset != null && Unit != null)
                _asset.AfterUpdate(Unit.Value, _world);
        }
    }
}