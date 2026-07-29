using System;
using System.Linq;
using ECS.Shared;
using OperationSystem.Component;

namespace ECS
{
    public readonly struct ComponentMask
    {
        private readonly BitCollection _typeBits;
        
        private ComponentMask(int singleTypeId)
        {
            _typeBits = new BitCollection(singleTypeId + 1);
            _typeBits.SetTrue(singleTypeId);
        }
        
        public static ComponentMask Create(params int[] typeIds)
        {
            if (typeIds.Length == 0)
                throw new InvalidOperationException();
            
            var mask = new ComponentMask(typeIds[0]);
            foreach (var id in typeIds.Skip(1)) 
				mask.Add(id);
            return mask;
        }

        public static ComponentMask Create<T>() where T : IComponent
        {
            return new ComponentMask(ComponentType<T>.Id);
        }

        public bool Contains(int typeId) => _typeBits.IsTrue(typeId);
        public bool Contains<T>() where T : IComponent => Contains(ComponentType<T>.Id);
        
        public void Add(int typeId) => _typeBits.SetTrue(typeId);
        public void Add<T>() where T : IComponent => Add(ComponentType<T>.Id);


        public void Remove(int typeId) => _typeBits.SetFalse(typeId);
        public void Remove<T>() where T : IComponent => Remove(ComponentType<T>.Id);

        public bool IsEmpty => !_typeBits.Any();
    }
}