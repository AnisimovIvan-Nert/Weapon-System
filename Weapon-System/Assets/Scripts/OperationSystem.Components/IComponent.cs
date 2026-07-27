using OperationSystem.Resource;

namespace OperationSystem.Components
{
    public interface IComponent : IResource
    {
        int DataSize { get; }
        byte[] GetData();
        void ReadData(byte[] data);
    }
}