using System;
using System.Collections;
using System.Runtime.ExceptionServices;
using OperationSystem.Assets;
using OperationSystem.Operations;
using OperationSystem.Units;

namespace OperationSystem.Handlers
{
    public abstract class AbstractOperationHandler : IOperationHandler
    {
        protected UnitWorld UnitWorld { get; }
        
        public Unit? OperationUnit { get; protected set; }
        public IOperationRunner OperationRunner { get; }

        protected AbstractOperationHandler(IOperationRunner operationRunner, UnitWorld unitWorld)
        {
            OperationRunner = operationRunner;
            UnitWorld = unitWorld;
        }
        
        public virtual void Update()
        {
            if (OperationUnit != null)
                UnitWorld.PullFromAssets(OperationUnit.Value);
            
            OperationRunner.Update();
            
            if (OperationUnit != null)
                UnitWorld.PushToAssets(OperationUnit.Value);
        }

        public virtual IOperationContext CreateContext() => new OperationContext(UnitWorld);
        
        public IEnumerator SetUnit(IAsset? asset)
        {
            var delayer = OperationRunner.DelayOperationRunning();
            
            if (OperationRunner.AnyRunningOperation)
                yield return null;

            try
            {
                if (asset == null)
                {
                    OperationUnit = null;
                }
                else
                {
                    if (!IsValidAsset(asset))
                        throw new InvalidOperationException();

                    OperationUnit = UnitWorld.Registry.Create(asset);
                }
            }
            catch (Exception e)
            {
                ExceptionDispatchInfo.Capture(e).Throw();
            }
            finally
            {
                OperationRunner.ReleaseOperationRunning(delayer);
            }
        }
        
        protected virtual bool IsValidAsset(IAsset asset) => true;
    }
}