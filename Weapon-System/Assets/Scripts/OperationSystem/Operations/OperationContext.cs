using System;
using System.Collections.Generic;
using System.Linq;
using OperationSystem.Assets;
using UnityEngine;

namespace OperationSystem.Operations
{
    public delegate void Undo();

    public interface IOperationContext : IDisposable
    {
        bool TryAcquire(IAsset asset, in OperationIdentifier owner);
        void RecordUndo(Undo undo);
        void Commit();
        void Rollback();
        void ReleaseAll();
    }

    public class OperationContext : IOperationContext
    {
        private readonly Dictionary<IAsset, OperationIdentifier> _locks = new();
        
        private readonly List<Undo> _undoStack = new();
        private bool _committed;
        
        public bool TryAcquire(IAsset asset, in OperationIdentifier owner)
        {
            if (!asset.TryLock(owner))
                return false;
            
            try
            {
                _locks[asset] = owner;
            }
            catch (Exception e)
            {
                asset.Release(owner);
                _locks.Remove(asset);
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
            foreach (var (asset, owner) in _locks)
            {
                try
                {
                    asset.Release(owner);
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