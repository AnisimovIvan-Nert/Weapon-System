using OperationSystem.Assets;
using OperationSystem.Component;
using OperationSystem.Component.Types;

namespace OperationSystem.Tests.Mocks
{
    public class FooAsset : AbstractAsset
    {
        public override ComponentMask GetComponentMask()
        {
            var mask = base.GetComponentMask();
            mask.Add<FooComponent>();
            return mask;
        }
    }

    public struct FooComponent : IComponent
    {
    }
}