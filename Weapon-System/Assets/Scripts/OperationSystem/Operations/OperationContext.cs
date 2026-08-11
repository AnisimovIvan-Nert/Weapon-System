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
        UnitWorld World { get; }
        
        bool TryAcquire<T>(in UnitId unitId, in OperationIdentifier owner) where T : struct, IComponent;
        bool TryAcquire(int typeId, in UnitId unitId, in OperationIdentifier owner);
        void RecordUndo(Undo undo);
        void Commit();
        void Rollback();
        void ReleaseAll();
    }

    public class OperationContext : IOperationContext
    {
        private readonly Dictionary<(UnitId unitId, int typeId), OperationIdentifier> _locks = new();
        
        private readonly List<Undo> _undoStack = new();
        private bool _committed;
        
        public UnitWorld World { get; }

        public OperationContext(UnitWorld unitWorld)
        {
            World = unitWorld;
        }

        public bool TryAcquire<T>(in UnitId unitId, in OperationIdentifier owner)
            where T : struct, IComponent
        {
            return TryAcquire(ComponentType<T>.Id, unitId, owner);
        }
        
        public bool TryAcquire(int typeId, in UnitId unitId, in OperationIdentifier owner)
        {
            if (!World.GetComponentArray(typeId).TryAcquireComponent(unitId, owner))
                return false;
            
            try
            {
                _locks[(unitId, typeId)] = owner;
            }
            catch (Exception e)
            {
                World.GetComponentArray(typeId).ReleaseComponent(unitId, owner);
                _locks.Remove((unitId, typeId));
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
                    World.GetComponentArray(typeId).ReleaseComponent(unitId, owner);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
            _locks.Clear();
        }
    }
}