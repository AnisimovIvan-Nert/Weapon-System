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
        private readonly List<int> _dataStartIndex;
        private readonly ShiftableDataCollection<byte> _data;
        
        public Components(TypeRegistry<IComponent> typeRegistry)
        {
            _typeRegistry = typeRegistry;
            
            _types = new List<int>(InitialSize);
            _dataStartIndex = new List<int>(InitialSize);
            _data = new ShiftableDataCollection<byte>(InitialDataSize);
        }

        public void AddComponent<T>(T component)
            where T : IComponent
        {
            var id = _typeRegistry.GetId<T>();
            var data = component.GetData();
            
            _types.Add(id);
            _dataStartIndex.Add(_data.Length);
            _data.Add(data);
        }

        public bool HasComponent<T>()
            where T : IComponent
        {
            foreach (var type in _types)
            {
                if (_typeRegistry.IsAssignableFrom<T>(type))
                    return true;
            }

            return false;
        }
    }
}