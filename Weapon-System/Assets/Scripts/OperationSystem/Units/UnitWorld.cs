using System;
using System.Collections.Generic;
using OperationSystem.Assets;
using OperationSystem.Component;
using OperationSystem.Component.Types;
using OperationSystem.Operations;
using OperationSystem.Operations.Middleware;

namespace OperationSystem.Units
{
    public readonly struct UnitWorld
    {
        private readonly UnitRegistry _registry;
        private readonly IComponentArray[] _componentArrays;
        private readonly List<IOperationMiddleware> _middlewares;
        
        public IOperationRunner OperationRunner { get; }

        public IEnumerable<IOperationMiddleware> Middlewares
        {
            get
            {
                lock (_middlewares)
                    return _middlewares.ToArray();
            }
        }

        private UnitWorld(IOperationRunner operationRunner, IComponentArray[] componentArrays)
        {
            OperationRunner = operationRunner;
            _componentArrays = componentArrays;
            _registry = new UnitRegistry();
            _middlewares = new List<IOperationMiddleware>();
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

        public void AppendMiddlewares(params IOperationMiddleware[] middlewares)
        {
            lock(_middlewares)
                _middlewares.AddRange(middlewares);
        }

        public void Update()
        {
            foreach (var unit in _registry.EnumerateUnits())
                PullFromAsset(unit);

            OperationRunner.Update();

            foreach (var unit in _registry.EnumerateUnits())
                PushToAsset(unit);
        }

        public Unit GetOrCreateUnit(IAsset asset)
        {
            return _registry.GetOrCreate(asset, this);
        }

        public void DestroyUnit(IAsset asset)
        {
            if (!_registry.Destroy(asset, out var unit))
                return;

            foreach (var typeId in unit.ComponentMask)
                GetComponentArray(typeId).DestroyComponent(unit);
        }

        internal IComponentArray GetComponentArray(int typeId) => _componentArrays[typeId];

        internal void OnUnitCreated(Unit unit) => PullFromAsset(unit);
        
        private void PullFromAsset(Unit unit)
        {
            foreach (var typeId in unit.ComponentMask)
                GetComponentArray(typeId).PullFromAsset(unit);
        }

        private void PushToAsset(Unit unit)
        {
            foreach (var typeId in unit.ComponentMask)
                GetComponentArray(typeId).PushToAsset(unit);
        }
    }
}