using System.Collections.Generic;
using Weapons.Assets;
using Weapons.Units.Attachments;
using Weapons.User;

namespace Weapons
{
    public class UnitAggregator : IUnit
    {
        public IUserAdapter User { get; }
        public List<IUnit> Units { get; }
        
        public UnitAggregator(IUserAdapter user, List<IUnit> units)
        {
            User = user;
            Units = units;
        }

        public static UnitAggregator Create(IUserAdapter userAdapter, IAsset asset)
        {
            var units = new List<IUnit>();

            var assetQueue = new Queue<IAsset>();
            assetQueue.Enqueue(asset);

            while (assetQueue.Count > 0)
            {
                var currentAsset = assetQueue.Dequeue();
                
                foreach (var child in currentAsset.Children)
                    assetQueue.Enqueue(child);

                var unit = currentAsset.ToUnit(userAdapter);
                units.Add(unit);
            }

            SetAttachmentNumbers(units);

            return new UnitAggregator(userAdapter, units);
        }
        
        public void Update()
        {
            foreach (var unit in Units)
                unit.Update();
        }
        
        private static void SetAttachmentNumbers(List<IUnit> units)
        {
            var attachmentNumber = 0;
            foreach (var unit in units)
            {
                if (unit is not IAttachment attachment)
                    continue;

                attachment.AttachmentNumber = attachmentNumber++;
            }
        }
    }
}