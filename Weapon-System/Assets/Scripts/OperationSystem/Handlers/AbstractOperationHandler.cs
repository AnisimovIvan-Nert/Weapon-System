using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using OperationSystem.Assets;
using OperationSystem.Operations;
using OperationSystem.Units;

namespace OperationSystem.Handlers
{
    public abstract class AbstractOperationHandler : IOperationHandler
    {
        public UnitWorld World { get; }
        public IOperationRunner OperationRunner { get; private set; }

        protected List<Unit> AllUnits { get; }
        
        protected AbstractOperationHandler(UnitWorld world, IOperationRunner? operationRunner)
        {
            AllUnits = new List<Unit>();
            OperationRunner = operationRunner ?? new OperationRunner();
            World = world;
        }

        public virtual void Update()
        {
            foreach (var unit in AllUnits)
                World.PullFromAssets(unit);

            OperationRunner.Update();

            foreach (var unit in AllUnits)
                World.PushToAssets(unit);
        }

        public virtual IOperationContext CreateContext() => new OperationContext(World);

        public IEnumerator SetUnit(IAsset? asset)
        {
            IOperationRunner.ILock? @lock;
            while (!OperationRunner.TryLockOperationRunning(out @lock))
                yield return null;

            if (@lock == null)
                throw new InvalidOperationException();

            while (OperationRunner.AnyRunningOperation)
                yield return null;

            Unit unit = default;

            try
            {
                World.Registry.Destroy(AllUnits);
                AllUnits.Clear();

                if (asset == null)
                    yield break;

                if (!IsValidAsset(asset))
                    throw new InvalidOperationException();

                unit = World.Registry.Create(asset, this);
            }
            catch (Exception e)
            {
                ExceptionDispatchInfo.Capture(e).Throw();
            }
            finally
            {
                OperationRunner.ReleaseOperationRunning(@lock);
            }

            yield return unit;
        }

        public void AppendChild(Unit unit)
        {
            AllUnits.Add(unit);
        }

        protected virtual bool IsValidAsset(IAsset asset) => true;
    }
}