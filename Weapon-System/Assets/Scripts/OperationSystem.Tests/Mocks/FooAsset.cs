using OperationSystem.Assets;
using OperationSystem.Component;
using OperationSystem.Component.Types;

namespace OperationSystem.Tests.Mocks
{
    public class FooAsset : AbstractAsset
    {
        public override ComponentMask GetComponentMask()
        {
            return ComponentMask.Create<FooComponent>();
        }
    }

    public struct FooComponent : IComponent
    {
    }
}