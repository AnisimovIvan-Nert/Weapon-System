using System;
using System.Collections;
using System.Collections.Generic;
using Coroutine.Instructions;
using OperationSystem.Operations.Data;

namespace OperationSystem.Operations.Abstract
{
    public abstract class AbstractOrchestratorOperation : AbstractOperation
    {
        private ICollection<IOperation>? _operations;
        private bool _waitingOrchestratedOperations;

        private ICollection<IOperation> Operations => _operations ?? throw new InvalidOperationException();

        protected AbstractOrchestratorOperation(OperationIdentifier identifier, params IOperationData[] data) 
            : base(identifier, data)
        {
        }
        
        protected abstract ICollection<IOperation> CreateAndRunOrchestratedOperations();

        protected override IEnumerator InitializationEnumerator()
        {
            yield return base.InitializationEnumerator();
            
            _operations = CreateAndRunOrchestratedOperations();
            
            foreach (var operation in Operations)
                yield return operation.RunStage(OperationStage.Initialization).WaitInstruction();
        }

        protected override IEnumerator ValidateEnumerator()
        {
            yield return base.ValidateEnumerator();
            
            foreach (var operation in Operations)
                yield return operation.RunStage(OperationStage.Validate).WaitInstruction();
        }

        protected override IEnumerator TryAcquireLocksEnumerator()
        {
            yield return base.TryAcquireLocksEnumerator();
            
            foreach (var operation in Operations)
                yield return operation.RunStage(OperationStage.TryAcquireLocks).WaitInstruction();
        }

        protected override IEnumerator ReleaseLocksEnumerator()
        {
            yield return base.ReleaseLocksEnumerator();
            
            foreach (var operation in Operations)
                yield return operation.RunStage(OperationStage.ReleaseLocks).WaitInstruction();
        }

        protected override IEnumerator RecordMutationsEnumerator()
        {
            yield return base.RecordMutationsEnumerator();
            
            foreach (var operation in Operations)
                yield return operation.RunStage(OperationStage.RecordMutations).WaitInstruction();
        }

        protected override IEnumerator ExecuteEnumerator()
        {
            yield return base.ExecuteEnumerator();
            
            foreach (var operation in Operations)
                yield return operation.RunStage(OperationStage.Execute).WaitInstruction();
        }

        protected override IEnumerator CompleteEnumerator()
        {
            yield return base.CompleteEnumerator();
            
            foreach (var operation in Operations)
                yield return operation.RunStage(OperationStage.Complete).WaitInstruction();
        }

        protected override IEnumerator CancelEnumerator()
        {
            yield return base.CancelEnumerator();
            
            foreach (var operation in Operations)
                yield return operation.RunStage(OperationStage.Cancel).WaitInstruction();
        }
    }
}