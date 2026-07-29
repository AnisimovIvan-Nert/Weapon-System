using System;
using System.Collections.Generic;
using OperationSystem.Component.Types;

namespace OperationSystem.Component
{
    public readonly struct ComponentsData
    {
        private readonly List<ComponentType> _types;
        private readonly List<IComponent> _components;

        private ComponentsData(bool _)
        {
            _types = new List<ComponentType>();
            _components = new List<IComponent>();
        }

        public static ComponentsData Create() => new(true);

        public void Clear()
        {
            _types.Clear();
            _components.Clear();
        }

        public void AddComponent<T>(T component)
            where T : IComponent
        {
            var componentType = ComponentType.Create(component.GetType());
            var index = _types.IndexOf(componentType);

            if (index != -1)
            {
                SetComponent(index, component);
                return;
            }

            _types.Add(componentType);
            _components.Add(component);
        }

        public void SetComponent<T>(T component)
            where T : IComponent
        {
            var componentType = ComponentType.Create<T>();
            var index = _types.IndexOf(componentType);

            if (index == -1)
                throw new InvalidOperationException();

            SetComponent(index, component);
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

        public bool HasComponent<T>()
            where T : IComponent
        {
            return IndexOf<T>() != -1;
        }
        
        public T? TryGet<T>()
            where T : IComponent
        {
            var index = IndexOf<T>();
            if (index == -1)
                return default;

            return _components[index] is T typedComponent 
                ? typedComponent 
                : throw new InvalidOperationException();
        }

        private void SetComponent<T>(int index, T component)
            where T : IComponent
        {
            _components[index] = component;
        }
    }
}