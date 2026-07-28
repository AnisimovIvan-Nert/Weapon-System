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
        bool TryAcquire(ComponentsData.ComponentResource resource, Guid owner);
        void RecordUndo(Undo undo);
        void Commit();
        void Rollback();
        void ReleaseAll();
        
        T? TryRead<T>(Guid owner) where T : IComponent;
        ComponentsData.ComponentAccess<T>? TryAccess<T>(Guid owner) where T : IComponent;
    }

    public class OperationContext : IOperationContext
    {
        private readonly Dictionary<ComponentsData.ComponentResource, Guid> _locks = new();

        private readonly List<Undo> _undoStack = new();
        private bool _committed;

        public bool TryAcquire(ComponentsData.ComponentResource resource, Guid owner)
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
        
        public T? TryRead<T>(Guid owner)
            where T : IComponent
        {
            var resource = TryGetResource<T>(owner);
            return resource == null ? default : resource.Value.Read<T>();
        }
        
        public ComponentsData.ComponentAccess<T>? TryAccess<T>(Guid owner)
            where T : IComponent
        {
            var resource = TryGetResource<T>(owner);
            return resource?.Access<T>();
        }

        private ComponentsData.ComponentResource? TryGetResource<T>(Guid owner)
            where T : IComponent
        {
            foreach (var (resource, lockOwner) in _locks)
            {
                if (resource.ComponentType.IsAssignableFrom<T>())
                    continue;
                
                if (lockOwner != owner)
                    continue;
                
                if (!resource.IsBelongs(owner))
                    continue;

                return resource;
            }
            
            return null;
        }
    }
}