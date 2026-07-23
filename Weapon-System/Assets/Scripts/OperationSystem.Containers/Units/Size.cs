using System.Linq;
using OperationSystem.Units;

namespace OperationSystem.Containers.Units
{
    public interface ISize : IUnit
    {
        int Height { get; }
        int Width { get; }
    }

    public class Size
        : AbstractUnit
        , ISize
    {
        public int Height { get; }
        public int Width { get; }
        
        public Size(int height, int width) 
            : base(Enumerable.Empty<IUnit>())
        {
            Height = height;
            Width = width;
        }
    }
}