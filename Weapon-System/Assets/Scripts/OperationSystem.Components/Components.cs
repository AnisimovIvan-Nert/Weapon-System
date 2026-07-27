using System;
using System.Collections.Generic;
using ShiftableData;

namespace OperationSystem.Components
{
    public readonly struct Components
    {
        private const int InitialSize = 64 / sizeof(int);
        private const int SingleDataSize = 4 * sizeof(int);
        private const int InitialDataSize = InitialSize * SingleDataSize;

        private readonly TypeRegistry<IComponent> _typeRegistry;

        private readonly List<int> _types;
        private readonly ComponentsData _data;

        public Components(TypeRegistry<IComponent> typeRegistry)
        {
            _typeRegistry = typeRegistry;

            _types = new List<int>(InitialSize);
            _data = ComponentsData.Create();
        }

        public void AddComponent<T>(T component)
            where T : IComponent
        {
            var id = _typeRegistry.GetId(component.GetType());
            var data = component.GetData();
            var index = _types.IndexOf(id);

            if (index != -1)
            {
                SetComponent(component);
                return;
            }

            _types.Add(id);
            _data.Add(_types.Count - 1, data.ToArray());
        }

        public void SetComponent<T>(T component)
            where T : IComponent
        {
            var id = _typeRegistry.GetId<T>();
            var data = component.GetData();
            var index = _types.IndexOf(id);

            if (index == -1)
                throw new InvalidOperationException();

            SetComponent(index, data.ToArray());
        }
        
        public int IndexOf<T>()
            where T : IComponent
        {
            for (var i = 0; i < _types.Count; i++)
            {
                var type = _types[i];
                if (_typeRegistry.IsAssignableFrom<T>(type))
                    return i;
            }

            return -1;
        }

        public bool HasComponent<T>()
            where T : IComponent
        {
            return IndexOf<T>() != -1;
        }

        public ComponentAccess<T>? TryAccessComponent<T>()
            where T : IComponent
        {
            var index = IndexOf<T>();
            if (index == -1)
                return null;

            var component = ReadComponent<T>(index);
            return new ComponentAccess<T>(index, component, this);
        }
        
        public T? TryReadComponent<T>()
            where T : IComponent
        {
            var index = IndexOf<T>();
            return index == -1 ? default : ReadComponent<T>(index);
        }
        
        private T ReadComponent<T>(int index)
            where T : IComponent
        {
            var id = _types[index];
            var type = _typeRegistry.GetType(id);
            var data = _data.Read(index);
            var component = (T)Activator.CreateInstance(type);
            component.ReadData(data);
            return component;
        }

        private void SetComponent(int index, byte[] data)
        {
            _data.Replace(index, data);
        }

        public struct ComponentAccess<T>
            where T : IComponent
        {
            private readonly int _index;
            private readonly Components _components;

            public T Component { get; set; }

            public ComponentAccess(int index, T component, Components components)
            {
                _index = index;
                _components = components;
                Component = component;
            }

            public void SaveChanges()
            {
                _components.SetComponent(_index, Component.GetData().ToArray());
            }
        }

        private readonly struct ComponentsData
        {
            private readonly List<int> _dataStartIndex;
            private readonly ShiftableDataCollection<byte> _data;
            
            private ComponentsData(bool dummy = true)
            {
                _dataStartIndex = new List<int>(InitialSize);
                _data = new ShiftableDataCollection<byte>(InitialDataSize);
            }

            public static ComponentsData Create() => new ComponentsData(true);

            public void Add(int index, byte[] data)
            {
                if (index != _dataStartIndex.Count)
                    throw new IndexOutOfRangeException();
                
                _dataStartIndex.Add(_data.Length);
                _data.Add(data);
            }

            public void Replace(int index, byte[] data)
            {
                var nextIndex = index + 1;
                var length = _data.Length;
                var (startIndex, endIndex) = GetBounds(index);
                _data.Replace(startIndex, endIndex, data);

                var difference = _data.Length - length;
            
                if (difference == 0)
                    return;

                for (var i = nextIndex; i < _dataStartIndex.Count; i++)
                    _dataStartIndex[i] += difference;
            }

            public Span<byte> Read(int index)
            {
                var (startIndex, endIndex) = GetBounds(index);
                return _data.GetRange(startIndex, endIndex);
            }

            public (int start, int end) GetBounds(int index)
            {
                var startIndex = _dataStartIndex[index];
                var endIndex = index + 1 >= _dataStartIndex.Count ? -1 : _dataStartIndex[index + 1];
                return (startIndex, endIndex);
            }
        }
    }
}