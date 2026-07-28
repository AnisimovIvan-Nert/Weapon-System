using System;
using System.Collections.Generic;
using OperationSystem.Component.Types;
using ShiftableData;

namespace OperationSystem.Component
{
    public readonly struct ComponentsData
    {
        private const int InitialSize = 64 / sizeof(int);
        private const int SingleDataSize = 4 * sizeof(int);
        private const int InitialDataSize = InitialSize * SingleDataSize;

        private readonly List<ComponentType> _types;
        private readonly List<Guid> _locks;
        private readonly Data _data;

        private ComponentsData(bool _)
        {
            _types = new List<ComponentType>(InitialSize);
            _data = Data.Create();
            _locks = new List<Guid>();
        }

        public static ComponentsData Create() => new(true);

        public void AddComponent<T>(T component)
            where T : IComponent
        {
            var componentType = ComponentType.Create(component.GetType());
            var data = component.GetData();
            var index = _types.IndexOf(componentType);

            if (index != -1)
            {
                SetComponent(component);
                return;
            }

            _types.Add(componentType);
            _locks.Add(Guid.Empty);
            _data.Add(_types.Count - 1, data.ToArray());
        }

        public void SetComponent<T>(T component)
            where T : IComponent
        {
            var componentType = ComponentType.Create<T>();
            var data = component.GetData();
            var index = _types.IndexOf(componentType);

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

        public bool IsDirty<T>()
            where T : IComponent
        {
            var index = IndexOf<T>();
            if (index == -1)
                throw new InvalidOperationException();

            return _data.IsDirty(index);
        }

        public ComponentResource? TryGetResource<T>()
            where T : IComponent
        {
            var index = IndexOf<T>();
            if (index == -1)
                return null;

            return new ComponentResource(index, this, _types[index]);
        }
        
        public ComponentAccess<T>? TryAccessComponent<T>()
            where T : IComponent
        {
            var index = IndexOf<T>();
            return index == -1 ? null : AccessComponent<T>(index);
        }

        public T? TryReadComponent<T>()
            where T : IComponent
        {
            var index = IndexOf<T>();
            return index == -1 ? default : ReadComponent<T>(index);
        }
        
        public Span<byte> ReadRaw<T>()
            where T : IComponent
        {
            var index = IndexOf<T>();
            return index == -1 ? throw new InvalidOperationException() : _data.Read(index);
        }

        private T ReadComponent<T>(int index)
            where T : IComponent
        {
            var componentType = _types[index];
            var type = componentType.ToType();
            var data = _data.Read(index);
            var component = (T)Activator.CreateInstance(type);
            component.ReadData(data);
            return component;
        }
        
        private ComponentAccess<T> AccessComponent<T>(int index)
            where T : IComponent
        {
            var component = ReadComponent<T>(index);
            return new ComponentAccess<T>(index, component, this);
        }

        private void SetComponent(int index, byte[] data)
        {
            _data.Replace(index, data);
        }

        private Guid GetLock(int index) => _locks[index];
        private void SetLock(int index, Guid guid) => _locks[index] = guid;

        public readonly struct ComponentResource : IEquatable<ComponentResource>
        {
            private readonly int _index;
            private readonly ComponentsData _componentsData;

            public ComponentType ComponentType { get; }
            public bool IsLocked => _componentsData.GetLock(_index) != Guid.Empty;

            public ComponentResource(int index, ComponentsData componentsData, ComponentType componentType)
            {
                _index = index;
                _componentsData = componentsData;
                ComponentType = componentType;
            }

            public T Read<T>()
                where T : IComponent
            {
                return _componentsData.ReadComponent<T>(_index);
            }
            
            public ComponentAccess<T> Access<T>()
                where T : IComponent
            {
                return _componentsData.AccessComponent<T>(_index);
            }

            public bool IsBelongs(Guid owner) => _componentsData.GetLock(_index) == owner;

            public bool TryAcquire(Guid owner)
            {
                var guid = _componentsData.GetLock(_index);
                if (guid == owner)
                    return true;

                if (guid != Guid.Empty)
                    return false;

                _componentsData.SetLock(_index, owner);
                return true;
            }

            public void Release(Guid owner)
            {
                var guid = _componentsData.GetLock(_index);
                if (guid == owner)
                    ForceRelease();

            }

            public void ForceRelease()
            {
                _componentsData.SetLock(_index, Guid.Empty);
            }

            public bool Equals(ComponentResource other) => _index == other._index;
            public override bool Equals(object? obj) => obj is ComponentResource other && Equals(other);
            public override int GetHashCode() => _index;
        }

        public struct ComponentAccess<T>
            where T : IComponent
        {
            private readonly int _index;
            private readonly ComponentsData _componentsData;

            public T Component { get; set; }

            public ComponentAccess(int index, T component, ComponentsData componentsData)
            {
                _index = index;
                _componentsData = componentsData;
                Component = component;
            }

            public void SaveChanges()
            {
                _componentsData.SetComponent(_index, Component.GetData().ToArray());
            }
        }

        private readonly struct Data
        {
            private readonly List<int> _dataStartIndex;
            private readonly List<bool> _dirty;
            private readonly ShiftableDataCollection<byte> _data;

            private Data(bool dummy = true)
            {
                _dataStartIndex = new List<int>(InitialSize);
                _dirty = new List<bool>(InitialSize);
                _data = new ShiftableDataCollection<byte>(InitialDataSize);
            }

            public static Data Create() => new Data(true);

            public void Add(int index, byte[] data)
            {
                if (index != _dataStartIndex.Count)
                    throw new IndexOutOfRangeException();

                _dataStartIndex.Add(_data.Length);
                _dirty.Add(false);
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

                _dirty[index] = true;
            }

            public Span<byte> Read(int index)
            {
                var (startIndex, endIndex) = GetBounds(index);
                return _data.GetRange(startIndex, endIndex);
            }

            public bool IsDirty(int index) => _dirty[index];

            private (int start, int end) GetBounds(int index)
            {
                var startIndex = _dataStartIndex[index];
                var endIndex = index + 1 >= _dataStartIndex.Count ? -1 : _dataStartIndex[index + 1];
                return (startIndex, endIndex);
            }
        }
    }
}