using System;
using System.Collections.Generic;
using System.Linq;
using OperationSystem.Resource;
using UnityEngine;

namespace OperationSystem.Operations
{
    public delegate void Undo();

    public interface IOperationContext : IDisposable
    {
        bool TryAcquire(IResource resource, object owner);
        void RecordUndo(Undo undo);
        void Commit();
        void Rollback();
        void ReleaseAll();
        
        T? TryAccessFirst<T>(object owner)
            where T : IResource;
    }

    public class OperationContext : IOperationContext
    {
        private readonly Dictionary<IResource, object> _locks = new();

        private readonly List<Undo> _undoStack = new();
        private bool _committed;

        public bool TryAcquire(IResource resource, object owner)
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
        
        public T? TryAccessFirst<T>(object owner)
            where T : IResource
        {
            foreach (var lockPair in _locks)
            {
                if (lockPair.Key is not T resource)
                    continue;
                
                if (lockPair.Value != owner)
                    continue;
                
                if (!resource.IsBelongs(owner))
                    continue;

                return resource;
            }
            
            return default;
        }
    }
}