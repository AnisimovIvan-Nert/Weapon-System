using System;

namespace OperationSystem.Component
{
    public abstract class AbstractComponent : IComponent
    {
        public Span<byte> GetData() => Span<byte>.Empty;
        public void ReadData(Span<byte> data) { }
    }
}