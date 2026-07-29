using System;
using OperationSystem.Component.Types;
using OperationSystem.Units;

namespace OperationSystem.Component
{
    public class ComponentHandle<T> : IComponentHandle
        where T : IComponent
    {
        private readonly IUnitTarget _asset;
        private readonly UnitWorld _unitWorld;
        
        private T _component;
        private bool _isDirty;

        public ComponentType Type { get; }

        public ComponentHandle(T component, IUnitTarget asset)
        {
            _component = component;
            _asset = asset;
            Type = ComponentType.Create<T>();
        }

        public TComponent Read<TComponent>() where TComponent : IComponent
        {
            if (_component is not TComponent typedComponent)
                throw new InvalidOperationException();

            return typedComponent;
        }

        public void Write<TComponent>(TComponent component) where TComponent : IComponent
        {
            if (component is not T typedComponent)
                throw new InvalidOperationException();

            _component = typedComponent;
            _isDirty = true;
        }

        public void PullData()
        {
            if (_asset.IsDirty(_component, _unitWorld))
                _component = _asset.PullData(_component, _unitWorld);
        }

        public void PushData()
        {
            if (_isDirty)
                _component = _asset.PullData(_component, _unitWorld);

            _isDirty = false;
        }
    }
}