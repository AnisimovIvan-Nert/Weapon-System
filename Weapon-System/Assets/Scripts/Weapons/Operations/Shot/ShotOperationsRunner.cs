using Weapons.Units.Weapons;
using Weapons.User.Events;

namespace Weapons.Operations.Shot
{
    public class ShotOperationsRunner : AbstractOperationsRunner<IWeapon>
    {
        private bool _shootInProgress;

        protected override void HandleEvents(IWeapon unit)
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
                Operations.Add(new ShotOperation());

            _shootInProgress &= !end;
        }
    }
}