using System.Collections.Generic;
using Weapons.Units.Weapons;
using Weapons.User.Events;

namespace Weapons.Operations.Shot
{
    public class ShotOperationsRunner : IOperationsRunner<IWeapon>
    {
        private readonly List<ShotOperation> _operations = new();

        private bool _shootInProgress;
        
        public void Update(IWeapon unit)
        {
            HandleEvents(unit);
            HandleOperations(unit);
        }

        private void HandleOperations(IWeapon unit)
        {
            foreach (var operation in _operations)
                operation.Increment(unit);

            _operations.RemoveAll(operation => operation.State == OperationState.Destroying);
        }

        private void HandleEvents(IWeapon unit)
        {
            var start = false;
            var end = false;
            foreach (var userEvent in unit.User.EnumerateEvents())
            {
                switch (userEvent)
                {
                    case ShootStartEvent:
                        start = true;
                        break;
                    case ShootEndEvent:
                        end = true;
                        break;
                }
            }

            _shootInProgress = start;
            
            if (_shootInProgress)
                _operations.Add(new ShotOperation());

            _shootInProgress &= !end;
        }
    }
}