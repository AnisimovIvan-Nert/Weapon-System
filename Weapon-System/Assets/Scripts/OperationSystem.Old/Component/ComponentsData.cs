using System.Collections.Generic;
using OperationSystem.Component.Types;
using OperationSystem.Operations;

namespace OperationSystem.Component
{
    public readonly struct ComponentsData
    {
        private readonly List<ComponentType> _types;
        private readonly List<ComponentResource> _components;

        private ComponentsData(bool _)
        {
            _types = new List<ComponentType>();
            _components = new List<ComponentResource>();
        }

        public static ComponentsData Create() => new(true);
        
        public void AddComponent(IComponentHandle handle)
        {
            var index = _types.IndexOf(handle.Type);
            var access = new ComponentResource(handle);

            if (index != -1)
            {
                _components[index] = access;
                return;
            }

            _types.Add(handle.Type);
            _components.Add(access);
        }

        public int IndexOf<T>()
            where T : IComponent
        {
            for (var i = 0; i < _types.Count; i++)
            {
                var type = _types[i];
                if (type.IsAssignableFrom<T>())
                    return i;
            }

            return -1;
        }
        
        public ComponentResource? TryGet<T>()
            where T : IComponent
        {
            var index = IndexOf<T>();
            if (index == -1)
                return null;

            return _components[index];
        }
        
        public T? TryRead<T>()
            where T : IComponent
        {
            var index = IndexOf<T>();
            if (index == -1)
                return default;

            return _components[index].Read<T>();
        }
        
        public void PullData()
        {
            foreach (var component in _components)
                component.PullData();
        }

        public void PushData()
        {
            foreach (var component in _components)
                component.PushData();
        }

        public class ComponentResource : IComponentResource
        {
            private readonly IComponentHandle _handle;
            
            private OperationIdentifier _owner;
            
            public bool IsLocked => _owner != default;
            
            public ComponentType Type => _handle.Type;

            public ComponentResource(IComponentHandle handle)
            {
                _handle = handle;
                _owner = default;
            }

            public T Read<T>() where T : IComponent
            {
                return _handle.Read<T>();
            }

            public void Write<T>(T component) where T : IComponent
            {
                _handle.Write(component);
            }

            public void PullData()
            {
                _handle.PullData();
            }

            public void PushData()
            {
                _handle.PushData();
            }

            public bool IsBelongs(OperationIdentifier owner) => _owner == owner;

            public bool TryAcquire(OperationIdentifier owner)
            {
                if (_owner == owner)
                    return true;
            
                if (IsLocked) 
                    return false;
            
                _owner = owner;
                return true;
            }

            public void Release(OperationIdentifier owner)
            {
                if (_owner == owner)
                    _owner = default;
            }
        }
    }
}