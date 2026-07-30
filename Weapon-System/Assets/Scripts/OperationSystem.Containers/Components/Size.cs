using OperationSystem.Component;

namespace OperationSystem.Containers.Components
{
    public interface ISize : IComponent
    {
        int Height { get; }
        int Width { get; }
    }

    public struct Size : ISize
    {
        public int Height { get; }
        public int Width { get; }
        
        public Size(int height, int width) 
        {
            Height = height;
            Width = width;
        }
    }
}