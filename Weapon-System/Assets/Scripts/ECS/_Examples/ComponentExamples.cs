using UnityEngine;

namespace ECS.Examples
{
    public struct TransformComponent : IComponent
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;
    }

    public struct HealthComponent : IComponent
    {
        public float Current;
        public float Max;
    }

    public struct InventoryComponent : IComponent
    {
        public ItemSlot[] Slots;
        public int Count;
    }

    public struct ProjectileComponent : IComponent
    {
        public Vector3 Velocity;
        public float Speed;
        public float Lifetime;
    }

    public struct WeaponComponent : IComponent
    {
        public float FireRate;
        public float Damage;
        public int Ammo;
    }
}