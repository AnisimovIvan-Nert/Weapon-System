using System.Collections.Generic;
using UnityEngine;

namespace ECS.Examples
{
    public class PlayerAsset : MonoBehaviour,
                               IAsset,
                               IAssetPull<TransformComponent>,
                               IAssetPush<TransformComponent>,
                               IAssetPull<HealthComponent>,
                               IAssetPush<HealthComponent>,
                               IAssetPull<InventoryComponent>,
                               IAssetPush<InventoryComponent>
    {
        public float healthCurrent = 100f;
        public float healthMax = 100f;
        public List<ItemSlot> inventorySlots = new(20);

        void IAssetPull<TransformComponent>.PullInto(ref TransformComponent c)
        {
            var t = transform;
            c.Position = t.position;
            c.Rotation = t.rotation;
            c.Scale = t.localScale;
        }

        void IAssetPush<TransformComponent>.PushFrom(in TransformComponent c)
        {
            var t = transform;
            t.position = c.Position;
            t.rotation = c.Rotation;
            t.localScale = c.Scale;
        }

        void IAssetPull<HealthComponent>.PullInto(ref HealthComponent c)
        {
            c.Current = healthCurrent;
            c.Max = healthMax;
        }

        void IAssetPush<HealthComponent>.PushFrom(in HealthComponent c)
        {
            healthCurrent = c.Current;
            healthMax = c.Max;
        }

        void IAssetPull<InventoryComponent>.PullInto(ref InventoryComponent c)
        {
            if (c.Slots == null || c.Slots.Length < inventorySlots.Count)
                c.Slots = new ItemSlot[inventorySlots.Count];
            inventorySlots.CopyTo(c.Slots);
            c.Count = inventorySlots.Count;
        }

        void IAssetPush<InventoryComponent>.PushFrom(in InventoryComponent c)
        {
            inventorySlots.Clear();
            for (var i = 0; i < c.Count; i++)
                inventorySlots.Add(c.Slots[i]);
        }
    }
}
