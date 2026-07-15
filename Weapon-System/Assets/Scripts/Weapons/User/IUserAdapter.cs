using System.Collections.Generic;
using Weapons.User.Events;

namespace Weapons.User
{
    public interface IUserAdapter
    {
        public void Update();
        
        IEnumerable<IUserEvent> EnumerateEvents();
    }
}