using System;
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
        public Unit? Unit { get; private set; }

        protected AbstractUnitOperationHandler(IOperationRunner operationRunner, UnitWorld unitWorld)
            : base(operationRunner, unitWorld)
        {
        }

        public IEnumerator SetUnit(IAsset? asset)
        {
            var delayer = OperationRunner.DelayOperationRunning();
            
            if (OperationRunner.AnyRunningOperation)
                yield return null;

            try
            {
                if (asset == null)
                {
                    Unit = null;
                }
                else
                {
                    if (!IsValidAsset(asset))
                        throw new InvalidOperationException();

                    Unit = UnitWorld.Registry.Create(asset);
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                OperationRunner.ReleaseOperationRunning(delayer);
            }
        }

        public override void Update()
        {
            if (Unit != null)
                UnitWorld.PullFromAssets(Unit.Value);
            base.Update();
            if (Unit != null)
                UnitWorld.PushToAssets(Unit.Value);
        }

        protected virtual bool IsValidAsset(IAsset asset) => true;
    }
}