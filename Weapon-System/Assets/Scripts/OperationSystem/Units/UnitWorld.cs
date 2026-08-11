using System;
using OperationSystem.Assets;
using OperationSystem.Component;
using OperationSystem.Component.Types;
using OperationSystem.Operations;

namespace OperationSystem.Units
{
    public readonly struct UnitWorld
    {
        private readonly UnitRegistry _registry;
        private readonly IOperationRunner _operationRunner;
        private readonly IComponentArray[] _componentArrays;

        private UnitWorld(IOperationRunner operationRunner, IComponentArray[] componentArrays)
        {
            _operationRunner = operationRunner;
            _componentArrays = componentArrays;
            _registry = new UnitRegistry();
        }

        public static UnitWorld Create(IOperationRunner? operationRunner = null)
        {
            operationRunner ??= new OperationRunner();
            var componentArrays = CreateComponentArrays();
            return new UnitWorld(operationRunner, componentArrays);

            IComponentArray[] CreateComponentArrays()
            {
                ComponentType.WarmUp();
                var count = ComponentType.RegisteredTypeCount;
                var result = new IComponentArray[count];
                for (var i = 0; i < count; i++)
                {
                    var type = ComponentType.GetType(i);
                    var arrayType = typeof(ComponentArray<>).MakeGenericType(type);
                    result[i] = (IComponentArray)Activator.CreateInstance(arrayType);
                }

                return result;
            }
        }

        public void Update()
        {
            foreach (var unit in _registry.EnumerateUnits())
                PullFromAssets(unit);

            _operationRunner.Update();

            foreach (var unit in _registry.EnumerateUnits())
                PushToAssets(unit);
        }

        public Unit GetOrCreateUnit(IAsset asset)
        {
            return _registry.GetOrCreate(asset);
        }

        public void DestroyUnit(IAsset asset)
        {
            if (!_registry.Destroy(asset, out var unit))
                return;

            foreach (var typeId in unit.ComponentMask)
                GetComponentArray(typeId).DestroyComponent(unit.Id);
        }

        public IComponentArray GetComponentArray(int typeId) => _componentArrays[typeId];

        internal void RunOperation(IOperation operation)
        {
            var context = new OperationContext(this);
            _operationRunner.RunOperation(operation, context);
        }

        private void PullFromAssets(Unit unit)
        {
            foreach (var typeId in unit.ComponentMask)
                GetComponentArray(typeId).PullFromAssets(unit);
        }

        private void PushToAssets(Unit unit)
        {
            foreach (var typeId in unit.ComponentMask)
                GetComponentArray(typeId).PushToAssets(unit);
        }
    }
}