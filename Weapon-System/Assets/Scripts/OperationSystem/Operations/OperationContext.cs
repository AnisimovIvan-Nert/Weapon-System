using System;
using System.Collections.Generic;
using System.Linq;
using OperationSystem.Component;
using OperationSystem.Component.Types;
using OperationSystem.Units;
using UnityEngine;

namespace OperationSystem.Operations
{
    public delegate void Undo();

    public interface IOperationContext : IDisposable
    {
        bool TryAcquire<T>(in UnitId unitId, in OperationIdentifier owner) where T : struct, IComponent;
        void RecordUndo(Undo undo);
        void Commit();
        void Rollback();
        void ReleaseAll();
        
        bool IsOwned<T>(in UnitId unitId, in OperationIdentifier owner) where T : struct, IComponent;
        T? TryGetReadOnly<T>(in UnitId unitId, in OperationIdentifier owner) where T : struct, IComponent;
        ref T GetRef<T>(in UnitId unitId, in OperationIdentifier owner) where T : struct, IComponent;
    }

    public class OperationContext : IOperationContext
    {
        private readonly Dictionary<(UnitId unitId, int typeId), OperationIdentifier> _locks = new();
        
        private readonly List<Undo> _undoStack = new();
        private readonly UnitWorld _unitWorld;
        private bool _committed;

        public OperationContext(UnitWorld unitWorld)
        {
            _unitWorld = unitWorld;
        }

        public bool TryAcquire<T>(in UnitId unitId, in OperationIdentifier owner)
            where T : struct, IComponent
        {
            ref var resourceOwner = ref _unitWorld.GetComponents<T>().GetOwner(unitId);

            if (resourceOwner != default && resourceOwner != owner)
                return false;

            resourceOwner = owner;
            _locks[(unitId, ComponentType<T>.Id)] = owner;

            try
            {
                resourceOwner = owner;
                _locks[(unitId, ComponentType<T>.Id)] = owner;
            }
            catch (Exception e)
            {
                resourceOwner = default;
                Debug.LogError(e);
                return false;
            }
            
            return true;
        }

        public void RecordUndo(Undo undo) => _undoStack.Add(undo);

        public void Commit()
        {
            _committed = true;
            _undoStack.Clear();
            
            ReleaseAll();
        }

        public void Rollback()
        {
            foreach (var undo in _undoStack.AsEnumerable().Reverse())
            {
                try
                {
                    undo.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
            _undoStack.Clear();
            
            ReleaseAll();
        }

        public void Dispose()
        {
            if (!_committed)
                Rollback();
        }

        public void ReleaseAll()
        {
            foreach (var ((unitId, typeId), owner) in _locks)
            {
                try
                {
                    ref var resourceOwner = ref _unitWorld.GetComponents(typeId).GetOwner(unitId);
                    if (resourceOwner == owner)
                        resourceOwner = default;
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
            _locks.Clear();
        }

        public bool IsOwned<T>(in UnitId unitId, in OperationIdentifier owner)
            where T : struct, IComponent
        {
            var typeId = ComponentType<T>.Id;
            if (!_locks.TryGetValue((unitId, typeId), out var resourceOwner))
                return false;

            if (resourceOwner != owner)
                return false;

            return true;
        }
        
        public T? TryGetReadOnly<T>(in UnitId unitId, in OperationIdentifier owner)
            where T : struct, IComponent
        {
            if (IsOwned<T>(unitId, owner))
                return null;
            
            return _unitWorld.GetComponents<T>().GetReadOnly(unitId);
        }
        
        public ref T GetRef<T>(in UnitId unitId, in OperationIdentifier owner)
            where T : struct, IComponent
        {
            if (IsOwned<T>(unitId, owner))
                throw new InvalidOperationException();
            
            return ref _unitWorld.GetComponents<T>().GetRef(unitId);
        }
    }
}