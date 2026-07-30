using OperationSystem.Assets;
using OperationSystem.Component.Types;
using OperationSystem.Containers.Components;

namespace OperationSystem.Containers.Tests.Mocks
{
    public class FooAsset 
        : AbstractAsset
        , IAssetPull<Size>
    {
        private readonly Size? _size;

        public FooAsset(Size? size = null)
        {
            _size = size;
        }

        public override ComponentMask GetComponentMask()
        {
            if (_size.HasValue)
                return ComponentMask.Create<Size>();

            return ComponentMask.Create();
        }
        
        public void PullInto(ref Size component)
        {
            if (_size.HasValue)
                component = _size.Value;
        }
    }
}