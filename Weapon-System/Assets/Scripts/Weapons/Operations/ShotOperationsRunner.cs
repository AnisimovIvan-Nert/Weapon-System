using System.Collections.Generic;
using Weapons.User.Events;

namespace Weapons.Operations
{
    public class ShotOperationsRunner : IOperationsRunner
    {
        private readonly List<IWeaponOperation> _operations = new();

        private bool _shootInProgress;
        
        public void Update(IWeapon weapon)
        {
            HandleEvents(weapon);
            HandleOperations(weapon);
        }

        private void HandleOperations(IWeapon weapon)
        {
            foreach (var operation in _operations)
                operation.Increment(weapon);

            _operations.RemoveAll(operation => operation.State == OperationState.Destroying);
        }

        private void HandleEvents(IWeapon weapon)
        {
            var start = false;
            var end = false;
            foreach (var userEvent in weapon.User.EnumerateEvents())
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