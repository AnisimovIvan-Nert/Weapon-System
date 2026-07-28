using System;

namespace OperationSystem.Component
{
    public interface IComponent
    {
        Span<byte> GetData();
        void ReadData(Span<byte> data);
    }
}