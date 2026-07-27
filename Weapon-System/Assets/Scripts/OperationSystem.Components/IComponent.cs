using System;
using OperationSystem.Resource;

namespace OperationSystem.Components
{
    public interface IComponent : IResource
    {
        Span<byte> GetData();
        void ReadData(Span<byte> data);
    }
}