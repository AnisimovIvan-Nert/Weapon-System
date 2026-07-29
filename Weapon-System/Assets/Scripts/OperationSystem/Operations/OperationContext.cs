using System;
using System.Collections.Generic;
using System.Linq;
using OperationSystem.Component;
using OperationSystem.Component.Types;
using UnityEngine;

namespace OperationSystem.Operations
{
    public delegate void Undo();

    public interface IOperationContext : IDisposable
    {
        bool TryAcquire<T>(T resource, OperationIdentifier owner) where T : IComponentResource;
        void RecordUndo(Undo undo);
        void Commit();
        void Rollback();
        void ReleaseAll();
        
        IComponentResource? TryAccess<T>(OperationIdentifier owner) where T : IComponent;
        T? TryRead<T>(OperationIdentifier owner) where T : IComponent;
    }

    public class OperationContext : IOperationContext
    {
        private readonly Dictionary<IComponentResource, OperationIdentifier> _locks = new();

        private readonly List<Undo> _undoStack = new();
        private bool _committed;

        public bool TryAcquire<T>(T resource, OperationIdentifier owner)
            where T : IComponentResource
        {
            if (!resource.TryAcquire(owner)) 
                return false;

            try
            {
                _locks[resource] = owner;
            }
            catch (Exception e)
            {
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
            foreach (var (resource, owner) in _locks)
            {
                try
                {
                    resource.Release(owner);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
            _locks.Clear();
        }
        
        public IComponentResource? TryAccess<T>(OperationIdentifier owner)
            where T : IComponent
        {
            foreach (var (resource, lockOwner) in _locks)
            {
                if (resource.Type.IsAssignableFrom<T>())
                    continue;
                
                if (lockOwner != owner)
                    continue;
                
                if (!resource.IsBelongs(owner))
                    continue;

                return resource;
            }
            
            return null;
        }
        
        public T? TryRead<T>(OperationIdentifier owner)
            where T : IComponent
        {
            var access = TryAccess<T>(owner);
            if (access == null)
                return default;
            return access.Read<T>();
        }
    }
}