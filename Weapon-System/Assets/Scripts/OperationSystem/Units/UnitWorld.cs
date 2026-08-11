using System;
using System.Collections.Generic;
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
        private readonly HashSet<Unit> _units;
        private readonly IComponentArray[] _componentArrays;
        private readonly object _unitsLock;

        private UnitWorld(IOperationRunner operationRunner, IComponentArray[] componentArrays)
        {
            _operationRunner = operationRunner;
            _componentArrays = componentArrays;
            _registry = new UnitRegistry();
            _units = new HashSet<Unit>();
            _unitsLock = new object();
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
            lock (_unitsLock)
                foreach (var unit in _units)
                    PullFromAssets(unit);
            
            _operationRunner.Update();
            
            lock (_unitsLock)
                foreach (var unit in _units)
                    PushToAssets(unit);
        }

        public Unit GetOrCreateUnit(IAsset asset)
        {
            var unit = _registry.GetOrCreate(asset);
            
            lock (_unitsLock)
                _units.Add(unit);
            
            return unit;
        }
        
        public void DestroyUnit(IAsset asset)
        {
            if (!_registry.Destroy(asset, out var unit)) 
                return;
            
            lock (_unitsLock)
                _units.Remove(unit);
            
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